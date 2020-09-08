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

        private readonly IAddressSpace _oamRam;

        private readonly IDisplay _display;

        private readonly InterruptManager _interruptManager;

        private readonly Dma _dma;
        
        private readonly ColorPalette _oamPalette;

        private readonly HBlankPhase _hBlankPhase;

        private readonly OamSearch _oamSearchPhase;

        private readonly PixelTransfer _pixelTransferPhase;

        private readonly VBlankPhase _vBlankPhase;

        private int _lcdEnabledDelay;

        private IGpuPhase _phase;

        public Gpu(IDisplay display, InterruptManager interruptManager, Dma dma, IAddressSpace oamRam, bool gbc)
        {
            Registers = new MemoryRegisters(GpuRegister.Values.Cast<MemoryRegisters.IRegister>());
            Lcdc = new Lcdc();
            _interruptManager = interruptManager;
            IsGbc = gbc;
            VideoRam0 = new Ram(0x8000, 0x2000);
            VideoRam1 = gbc ? new Ram(0x8000, 0x2000) : null;

            _oamRam = oamRam;
            _dma = dma;

            BgPalette = new ColorPalette(0xff68);
            _oamPalette = new ColorPalette(0xff6a);
            _oamPalette.FillWithFF();

            _oamSearchPhase = new OamSearch(oamRam, Lcdc, Registers);
            _pixelTransferPhase = new PixelTransfer(VideoRam0, VideoRam1, oamRam, display, Lcdc, Registers, gbc, BgPalette, _oamPalette);
            _hBlankPhase = new HBlankPhase();
            _vBlankPhase = new VBlankPhase();

            Mode = GpuMode.OamSearch;
            _phase = _oamSearchPhase.Start();

            _display = display;
        }

        private IAddressSpace? GetAddressSpace(int address)
        {
            if (VideoRam0.Accepts(address)/* && mode != Mode.PixelTransfer*/)
            {
                return VideoRam;
            }

            if (_oamRam.Accepts(address) && !_dma.IsOamBlocked /* && mode != Mode.OamSearch && mode != Mode.PixelTransfer*/)
            {
                return _oamRam;
            }

            if (Lcdc.Accepts(address))
            {
                return Lcdc;
            }

            if (Registers.Accepts(address))
            {
                return Registers;
            }

            if (IsGbc && BgPalette.Accepts(address))
            {
                return BgPalette;
            }

            if (IsGbc && _oamPalette.Accepts(address))
            {
                return _oamPalette;
            }

            return null;
        }

        private IAddressSpace? VideoRam
        {
            get
            {
                if (IsGbc && (Registers.Get(VBK) & 1) == 1)
                {
                    return VideoRam1;
                }

                return VideoRam0;
            }
        }

        public IAddressSpace VideoRam0 { get; }

        public IAddressSpace? VideoRam1 { get; }

        public bool Accepts(int address) => GetAddressSpace(address) != null;

        public int this[int address]
        {
            get
            {
                if (address == STAT.Address)
                {
                    return Stat;
                }

                var space = GetAddressSpace(address);
                
                if (space == null)
                {
                    return 0xff;
                }

                if (address == VBK.Address)
                {
                    return IsGbc ? 0xfe : 0xff;
                }

                return space[address];
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

                    if (space == Lcdc)
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
            if (!IsLcdEnabled)
            {
                if (_lcdEnabledDelay != -1)
                {
                    if (--_lcdEnabledDelay == 0)
                    {
                        _display.EnableLcd();
                        IsLcdEnabled = true;
                    }
                }
            }
            if (!IsLcdEnabled)
            {
                return null;
            }

            var oldMode = Mode;
            TicksInLine++;
            if (_phase.Tick())
            {
                // switch line 153 to 0
                if (TicksInLine == 4 && Mode == GpuMode.VBlank && Registers.Get(LY) == 153)
                {
                    Registers.Put(LY, 0);
                    RequestLycEqualsLyInterrupt();
                }
            }
            else
            {
                switch (oldMode)
                {
                    case GpuMode.OamSearch:
                        Mode = GpuMode.PixelTransfer;
                        _phase = _pixelTransferPhase.Start(_oamSearchPhase.Sprites);
                        break;

                    case GpuMode.PixelTransfer:
                        Mode = GpuMode.HBlank;
                        _phase = _hBlankPhase.Start(TicksInLine);
                        RequestLcdcInterrupt(3);
                        break;

                    case GpuMode.HBlank:
                        TicksInLine = 0;
                        if (Registers.PreIncrement(LY) == 144)
                        {
                            Mode = GpuMode.VBlank;
                            _phase = _vBlankPhase.Start();
                            _interruptManager.RequestInterrupt(InterruptType.VBlank);
                            RequestLcdcInterrupt(4);
                        }
                        else
                        {
                            Mode = GpuMode.OamSearch;
                            _phase = _oamSearchPhase.Start();
                        }
                        RequestLcdcInterrupt(5);
                        RequestLycEqualsLyInterrupt();
                        break;

                    case GpuMode.VBlank:
                        TicksInLine = 0;
                        if (Registers.PreIncrement(LY) == 1)
                        {
                            Mode = GpuMode.OamSearch;
                            Registers.Put(LY, 0);
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

            if (oldMode == Mode)
            {
                return null;
            }

            return Mode;
        }

        public int TicksInLine { get; private set; }

        private void RequestLcdcInterrupt(int statBit)
        {
            if ((Registers.Get(STAT) & (1 << statBit)) != 0)
            {
                _interruptManager.RequestInterrupt(InterruptType.LCDC);
            }
        }

        private void RequestLycEqualsLyInterrupt()
        {
            if (Registers.Get(LYC) == Registers.Get(LY))
            {
                RequestLcdcInterrupt(6);
            }
        }

        public int Stat
        {
            get => Registers.Get(STAT) | Mode.Ordinal() | (Registers.Get(LYC) == Registers.Get(LY) ? (1 << 2) : 0) | 0x80;
            set => Registers.Put(STAT, value & 0b11111000); // last three bits are read-only
        }

        private void SetLcdc(int value)
        {
            Lcdc.Value = value;
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
            Registers.Put(LY, 0);
            TicksInLine = 0;
            _phase = _hBlankPhase.Start(250);
            Mode = GpuMode.HBlank;
            IsLcdEnabled = false;
            _lcdEnabledDelay = -1;
            _display.DisableLcd();
        }

        private void EnableLcd()
        {
            _lcdEnabledDelay = 244;
        }

        public bool IsLcdEnabled { get; private set; } = true;

        public Lcdc Lcdc { get; }

        public MemoryRegisters Registers { get; }

        public bool IsGbc { get; }

        public ColorPalette BgPalette { get; }

        public GpuMode Mode { get; private set; }
    }
}