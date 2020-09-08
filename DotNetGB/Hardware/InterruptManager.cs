using DotNetGB.Util;

namespace DotNetGB.Hardware
{
    public class InterruptManager : IAddressSpace
    {
        public enum InterruptType
        {
            VBlank = 0x0040,
            LCDC = 0x0048,
            Timer = 0x0050,
            Serial = 0x0058,
            P10_13 = 0x0060,
        }
        
        private readonly bool _gbc;
        
        public bool IsIme { get; private set; }

        private int _interruptFlag = 0xe1;

        private int _interruptEnabled;

        private int _pendingEnableInterrupts = -1;

        private int _pendingDisableInterrupts = -1;

        public InterruptManager(bool gbc)
        {
            _gbc = gbc;
        }

        public void EnableInterrupts(bool withDelay)
        {
            _pendingDisableInterrupts = -1;
            if (withDelay)
            {
                if (_pendingEnableInterrupts == -1)
                {
                    _pendingEnableInterrupts = 1;
                }
            }
            else
            {
                _pendingEnableInterrupts = -1;
                IsIme = true;
            }
        }

        public void DisableInterrupts(bool withDelay)
        {
            _pendingEnableInterrupts = -1;
            if (withDelay && _gbc)
            {
                if (_pendingDisableInterrupts == -1)
                {
                    _pendingDisableInterrupts = 1;
                }
            }
            else
            {
                _pendingDisableInterrupts = -1;
                IsIme = false;
            }
        }

        public void RequestInterrupt(InterruptType type)
        {
            _interruptFlag |= (1 << type.Ordinal());
        }

        public void ClearInterrupt(InterruptType type)
        {
            _interruptFlag &= ~(1 << type.Ordinal());
        }

        public void OnInstructionFinished()
        {
            if (_pendingEnableInterrupts != -1)
            {
                if (_pendingEnableInterrupts-- == 0)
                {
                    EnableInterrupts(false);
                }
            }
            if (_pendingDisableInterrupts != -1)
            {
                if (_pendingDisableInterrupts-- == 0)
                {
                    DisableInterrupts(false);
                }
            }
        }

        public bool IsInterruptRequested => (_interruptFlag & _interruptEnabled) != 0;

        public bool IsHaltBug => (_interruptFlag & _interruptEnabled & 0x1f) != 0 && IsIme;

        public bool Accepts(int address) => address == 0xff0f || address == 0xffff;

        public int this[int address]
        {
            get
            {
                switch (address)
                {
                    case 0xff0f:
                        return _interruptFlag;

                    case 0xffff:
                        return _interruptEnabled;

                    default:
                        return 0xff;
                }
            }
            set 
            {
                switch (address)
                {
                    case 0xff0f:
                        _interruptFlag = value | 0xe0;
                        break;

                    case 0xffff:
                        _interruptEnabled = value;
                        break;
                }
            }
}
    }
}