using System;

namespace DotNetGB.Hardware
{
    public class Hdma : IAddressSpace, ITickable
    {
        private const int HDMA1 = 0xff51;

        private const int HDMA2 = 0xff52;

        private const int HDMA3 = 0xff53;

        private const int HDMA4 = 0xff54;

        private const int HDMA5 = 0xff55;

        private readonly IAddressSpace _addressSpace;

        private readonly Ram _hdma1234 = new Ram(HDMA1, 4);

        private Gpu.GpuMode? _gpuMode;

        private bool _transferInProgress;

        private bool _hblankTransfer;

        private bool _lcdEnabled;

        private int _length;

        private int _src;

        private int _dst;

        private int _tick;

        public Hdma(IAddressSpace addressSpace)
        {
            _addressSpace = addressSpace;
        }

        public bool Accepts(int address) => address >= HDMA1 && address <= HDMA5;

        public void Tick()
        {
            if (!IsTransferInProgress)
            {
                return;
            }
            if (++_tick < 0x20)
            {
                return;
            }
            for (int j = 0; j < 0x10; j++)
            {
                _addressSpace[_dst + j] = _addressSpace[_src + j];
            }
            _src += 0x10;
            _dst += 0x10;
            if (_length-- == 0)
            {
                _transferInProgress = false;
                _length = 0x7f;
            }
            else if (_hblankTransfer)
            {
                _gpuMode = null; // wait until next HBlank
            }
        }

        public int this[int address]
        {
            get
            {
                if (_hdma1234.Accepts(address))
                {
                    return 0xff;
                }
                else if (address == HDMA5)
                {
                    return (_transferInProgress ? 0 : (1 << 7)) | _length;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            set
            {
                if (_hdma1234.Accepts(address))
                {
                    _hdma1234[address] = value;
                }
                else if (address == HDMA5)
                {
                    if (_transferInProgress && (address & (1 << 7)) == 0)
                    {
                        StopTransfer();
                    }
                    else
                    {
                        StartTransfer(value);
                    }
                }
            }
        }

        public void OnGpuUpdate(Gpu.GpuMode? newGpuMode)
        {
            _gpuMode = newGpuMode;
        }

        public void OnLcdSwitch(bool lcdEnabled)
        {
            _lcdEnabled = lcdEnabled;
        }

        public bool IsTransferInProgress
        {
            get
            {
                if (!_transferInProgress)
                {
                    return false;
                }
                else if (_hblankTransfer && (_gpuMode == Gpu.GpuMode.HBlank || !_lcdEnabled))
                {
                    return true;
                }
                else if (!_hblankTransfer)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private void StartTransfer(int reg)
        {
            _hblankTransfer = (reg & (1 << 7)) != 0;
            _length = reg & 0x7f;

            _src = (_hdma1234[HDMA1] << 8) | (_hdma1234[HDMA2] & 0xf0);
            _dst = ((_hdma1234[HDMA3] & 0x1f) << 8) | (_hdma1234[HDMA4] & 0xf0);
            _src &= 0xfff0;
            _dst = (_dst & 0x1fff) | 0x8000;

            _transferInProgress = true;
        }

        private void StopTransfer()
        {
            _transferInProgress = false;
        }
    }
}
