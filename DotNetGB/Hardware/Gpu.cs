using System.Linq;
using DotNetGB.Hardware.GpuPhases;
using DotNetGB.Util;
using static DotNetGB.Hardware.GpuRegister;
using InterruptType = DotNetGB.Hardware.InterruptManager.InterruptType;

namespace DotNetGB.Hardware
{
    public class Gpu : IAddressSpace
    {
        public enum GpuMode
        {
            HBlank, VBlank, OamSearch, PixelTransfer
        }

        private readonly IAddressSpace _videoRam0;

        private readonly IAddressSpace? _videoRam1;

        private readonly IAddressSpace _oamRam;

        private readonly IDisplay _display;

        private readonly InterruptManager _interruptManager;

        private readonly Dma _dma;

        private readonly Lcdc _lcdc;

        private readonly bool _gbc;

        private readonly ColorPalette _bgPalette;

        private readonly ColorPalette _oamPalette;

        private readonly HBlankPhase _hBlankPhase;

        private readonly OamSearch _oamSearchPhase;

        private readonly PixelTransfer _pixelTransferPhase;

        private readonly VBlankPhase _vBlankPhase;

        private bool _lcdEnabled = true;

        private int _lcdEnabledDelay;

        private MemoryRegisters _r;

        private int _ticksInLine;

        private GpuMode _mode;

        private IGpuPhase _phase;

        public Gpu(IDisplay display, InterruptManager interruptManager, Dma dma, Ram oamRam, bool gbc)
        {
            _r = new MemoryRegisters(GpuRegister.Values.Cast<MemoryRegisters.IRegister>());
            _lcdc = new Lcdc();
            _interruptManager = interruptManager;
            _gbc = gbc;
            _videoRam0 = new Ram(0x8000, 0x2000);
            if (gbc)
            {
                _videoRam1 = new Ram(0x8000, 0x2000);
            }
            else
            {
                _videoRam1 = null;
            }
            _oamRam = oamRam;
            _dma = dma;

            _bgPalette = new ColorPalette(0xff68);
            _oamPalette = new ColorPalette(0xff6a);
            _oamPalette.FillWithFF();

            _oamSearchPhase = new OamSearch(oamRam, _lcdc, _r);
            _pixelTransferPhase = new PixelTransfer(_videoRam0, _videoRam1, oamRam, display, _lcdc, _r, gbc, _bgPalette, _oamPalette);
            _hBlankPhase = new HBlankPhase();
            _vBlankPhase = new VBlankPhase();

            _mode = GpuMode.OamSearch;
            _phase = _oamSearchPhase.Start();

            _display = display;
        }

        private IAddressSpace? GetAddressSpace(int address)
        {
            if (_videoRam0.Accepts(address)/* && mode != Mode.PixelTransfer*/)
            {
                return VideoRam;
            }
            else if (_oamRam.Accepts(address) && !_dma.IsOamBlocked /* && mode != Mode.OamSearch && mode != Mode.PixelTransfer*/)
            {
                return _oamRam;
            }
            else if (_lcdc.Accepts(address))
            {
                return _lcdc;
            }
            else if (_r.Accepts(address))
            {
                return _r;
            }
            else if (_gbc && _bgPalette.Accepts(address))
            {
                return _bgPalette;
            }
            else if (_gbc && _oamPalette.Accepts(address))
            {
                return _oamPalette;
            }
            else
            {
                return null;
            }
        }

        private IAddressSpace VideoRam
        {
            get
            {
                if (_gbc && (_r.Get(VBK) & 1) == 1)
                {
                    return _videoRam1;
                }
                else
                {
                    return _videoRam0;
                }
            }
        }

        public IAddressSpace VideoRam0 => _videoRam0;

        public IAddressSpace? VideoRam1 => _videoRam1;

        public bool Accepts(int address) => GetAddressSpace(address) != null;

        public int this[int address]
        {
            get
            {
                if (address == STAT.Address)
                {
                    return Stat;
                }
                else
                {
                    var space = GetAddressSpace(address);
                    if (space == null)
                    {
                        return 0xff;
                    }
                    else if (address == VBK.Address)
                    {
                        return _gbc ? 0xfe : 0xff;
                    }
                    else
                    {
                        return space[address];
                    }
                }
            }
            set
            {
                if (address == STAT.Address)
                {
                    Stat = value;
                }
                else
                {
                    var space = GetAddressSpace(address);
                    if (space == _lcdc)
                    {
                        SetLcdc(value);
                    }
                    else if (space != null)
                    {
                        space[address] = value;
                    }
                }
            }
        }

        public GpuMode? Tick()
        {
            if (!_lcdEnabled)
            {
                if (_lcdEnabledDelay != -1)
                {
                    if (--_lcdEnabledDelay == 0)
                    {
                        _display.EnableLcd();
                        _lcdEnabled = true;
                    }
                }
            }
            if (!_lcdEnabled)
            {
                return null;
            }

            GpuMode oldMode = _mode;
            _ticksInLine++;
            if (_phase.Tick())
            {
                // switch line 153 to 0
                if (_ticksInLine == 4 && _mode == GpuMode.VBlank && _r.Get(LY) == 153)
                {
                    _r.Put(LY, 0);
                    RequestLycEqualsLyInterrupt();
                }
            }
            else
            {
                switch (oldMode)
                {
                    case GpuMode.OamSearch:
                        _mode = GpuMode.PixelTransfer;
                        _phase = _pixelTransferPhase.Start(_oamSearchPhase.Sprites);
                        break;

                    case GpuMode.PixelTransfer:
                        _mode = GpuMode.HBlank;
                        _phase = _hBlankPhase.Start(_ticksInLine);
                        RequestLcdcInterrupt(3);
                        break;

                    case GpuMode.HBlank:
                        _ticksInLine = 0;
                        if (_r.PreIncrement(LY) == 144)
                        {
                            _mode = GpuMode.VBlank;
                            _phase = _vBlankPhase.Start();
                            _interruptManager.RequestInterrupt(InterruptType.VBlank);
                            RequestLcdcInterrupt(4);
                        }
                        else
                        {
                            _mode = GpuMode.OamSearch;
                            _phase = _oamSearchPhase.Start();
                        }
                        RequestLcdcInterrupt(5);
                        RequestLycEqualsLyInterrupt();
                        break;

                    case GpuMode.VBlank:
                        _ticksInLine = 0;
                        if (_r.PreIncrement(LY) == 1)
                        {
                            _mode = GpuMode.OamSearch;
                            _r.Put(LY, 0);
                            _phase = _oamSearchPhase.Start();
                            RequestLcdcInterrupt(5);
                        }
                        else
                        {
                            _phase = _vBlankPhase.Start();
                        }
                        RequestLycEqualsLyInterrupt();
                        break;
                }
            }
            if (oldMode == _mode)
            {
                return null;
            }
            else
            {
                return _mode;
            }
        }

        public int TicksInLine => _ticksInLine;

        private void RequestLcdcInterrupt(int statBit)
        {
            if ((_r.Get(STAT) & (1 << statBit)) != 0)
            {
                _interruptManager.RequestInterrupt(InterruptType.LCDC);
            }
        }

        private void RequestLycEqualsLyInterrupt()
        {
            if (_r.Get(LYC) == _r.Get(LY))
            {
                RequestLcdcInterrupt(6);
            }
        }

        public int Stat
        {
            get => _r.Get(STAT) | _mode.Ordinal() | (_r.Get(LYC) == _r.Get(LY) ? (1 << 2) : 0) | 0x80;
            set => _r.Put(STAT, value & 0b11111000); // last three bits are read-only
        }

        private void SetLcdc(int value)
        {
            _lcdc.Value = value;
            if ((value & (1 << 7)) == 0)
            {
                DisableLcd();
            }
            else
            {
                EnableLcd();
            }
        }

        private void DisableLcd()
        {
            _r.Put(LY, 0);
            _ticksInLine = 0;
            _phase = _hBlankPhase.Start(250);
            _mode = GpuMode.HBlank;
            _lcdEnabled = false;
            _lcdEnabledDelay = -1;
            _display.DisableLcd();
        }

        private void EnableLcd()
        {
            _lcdEnabledDelay = 244;
        }

        public bool IsLcdEnabled => _lcdEnabled;

        public Lcdc Lcdc
        {
            get => _lcdc;
        }

        public MemoryRegisters Registers => _r;

        public bool IsGbc => _gbc;

        public ColorPalette BgPalette => _bgPalette;

        public GpuMode Mode => _mode;
    }
}