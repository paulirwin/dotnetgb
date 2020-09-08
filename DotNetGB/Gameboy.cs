using System;
using System.Collections.Generic;
using DotNetGB.Hardware;
using DotNetGB.Hardware.Sounds;
using DotNetGB.Util;

using Console = DotNetGB.Debug.Console;

namespace DotNetGB
{
    public class Gameboy
    {
        public const int TICKS_PER_SEC = 4_194_304;

        private readonly Gpu _gpu;

        private readonly Mmu _mmu;

        private readonly Cpu _cpu;

        private readonly Timer _timer;

        private readonly Dma _dma;

        private readonly Hdma _hdma;

        private readonly IDisplay _display;

        private readonly Sound _sound;

        //private readonly SerialPort _serialPort;

        private readonly bool _gbc;

        private readonly SpeedMode _speedMode;

        private readonly Optional<Console> _console;

        private volatile bool _doStop;

        private readonly IList<Action> _tickListeners = new List<Action>();

        public Gameboy(GameboyOptions options, Cartridge rom, IDisplay display, IController controller, ISoundOutput soundOutput, ISerialEndpoint serialEndpoint)
            : this(options, rom, display, controller, soundOutput, serialEndpoint, Optional<Console>.Empty())
        {
        }

        public Gameboy(GameboyOptions options, Cartridge rom, IDisplay display, IController controller, ISoundOutput soundOutput, ISerialEndpoint serialEndpoint, Optional<Console> console)
        {
            _display = display;
            _gbc = rom.IsGbc;
            _speedMode = new SpeedMode();
            InterruptManager interruptManager = new InterruptManager(_gbc);
            _timer = new Timer(interruptManager, _speedMode);
            _mmu = new Mmu();

            Ram oamRam = new Ram(0xfe00, 0x00a0);
            _dma = new Dma(_mmu, oamRam, _speedMode);
            _gpu = new Gpu(display, interruptManager, _dma, oamRam, _gbc);
            _hdma = new Hdma(_mmu);
            _sound = new Sound(soundOutput, _gbc);
            //_serialPort = new SerialPort(_interruptManager, serialEndpoint, _speedMode);
            _mmu.AddAddressSpace(rom);
            _mmu.AddAddressSpace(_gpu);
            _mmu.AddAddressSpace(new Joypad(interruptManager, controller));
            _mmu.AddAddressSpace(interruptManager);
            //_mmu.AddAddressSpace(_serialPort);
            _mmu.AddAddressSpace(_timer);
            _mmu.AddAddressSpace(_dma);
            _mmu.AddAddressSpace(_sound);

            _mmu.AddAddressSpace(new Ram(0xc000, 0x1000));
            if (_gbc)
            {
                _mmu.AddAddressSpace(_speedMode);
                _mmu.AddAddressSpace(_hdma);
                _mmu.AddAddressSpace(new GbcRam());
                _mmu.AddAddressSpace(new UndocumentedGbcRegisters());
            }
            else
            {
                _mmu.AddAddressSpace(new Ram(0xd000, 0x1000));
            }
            _mmu.AddAddressSpace(new Ram(0xff80, 0x7f));
            _mmu.AddAddressSpace(new ShadowAddressSpace(_mmu, 0xe000, 0xc000, 0x1e00));

            _cpu = new Cpu(_mmu, interruptManager, _gpu, display, _speedMode);

            interruptManager.DisableInterrupts(false);
            if (!options.UseBootstrap)
            {
                InitRegs();
            }

            _console = console;
        }

        private void InitRegs()
        {
            var r = _cpu.Registers;

            r.AF = 0x01b0;
            if (_gbc)
            {
                r.A = 0x11;
            }
            r.BC = 0x0013;
            r.DE = 0x00d8;
            r.HL = 0x014d;
            r.SP = 0xfffe;
            r.PC = 0x0100;
        }

        public void Run()
        {
            bool requestedScreenRefresh = false;
            bool lcdDisabled = false;
            _doStop = false;
            while (!_doStop)
            {
                Gpu.GpuMode? newMode = Tick();
                if (newMode != null)
                {
                    _hdma.OnGpuUpdate(newMode);
                }

                if (!lcdDisabled && !_gpu.IsLcdEnabled)
                {
                    lcdDisabled = true;
                    _display.RequestRefresh();
                    _hdma.OnLcdSwitch(false);
                }
                else if (newMode == Gpu.GpuMode.VBlank)
                {
                    requestedScreenRefresh = true;
                    _display.RequestRefresh();
                }

                if (lcdDisabled && _gpu.IsLcdEnabled)
                {
                    lcdDisabled = false;
                    _display.WaitForRefresh();
                    _hdma.OnLcdSwitch(true);
                }
                else if (requestedScreenRefresh && newMode == Gpu.GpuMode.OamSearch)
                {
                    requestedScreenRefresh = false;
                    _display.WaitForRefresh();
                }
                //_console.IfPresent(Console.Tick);
                _tickListeners.ForEach();
            }
        }

        public void Stop()
        {
            _doStop = true;
        }

        private Gpu.GpuMode? Tick()
        {
            _timer.Tick();
            if (_hdma.IsTransferInProgress)
            {
                _hdma.Tick();
            }
            else
            {
                _cpu.Tick();
            }
            _dma.Tick();
            _sound.Tick();
            //_serialPort.Tick();
            return _gpu.Tick();
        }

        public IAddressSpace AddressSpace => _mmu;

        public Cpu Cpu => _cpu;

        public SpeedMode SpeedMode => _speedMode;

        public Gpu Gpu => _gpu;

        public void RegisterTickListener(Action tickListener)
        {
            _tickListeners.Add(tickListener);
        }

        public void UnregisterTickListener(Action tickListener)
        {
            _tickListeners.Remove(tickListener);
        }

        public Sound Sound => _sound;
    }
}
