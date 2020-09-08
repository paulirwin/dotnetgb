namespace DotNetGB.Hardware
{
    public class SpeedMode : IAddressSpace
    {
        private bool _currentSpeed;
        private bool _prepareSpeedSwitch;

        public bool Accepts(int address) => address == 0xff4d;

        public int this[int address]
        {
            get => _currentSpeed ? (1 << 7) : 0 | (_prepareSpeedSwitch ? (1 << 0) : 0) | 0b01111110;
            set => _prepareSpeedSwitch = (value & 0x01) != 0;
        }

        internal bool OnStop()
        {
            if (_prepareSpeedSwitch)
            {
                _currentSpeed = !_currentSpeed;
                _prepareSpeedSwitch = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        public int Mode => _currentSpeed ? 2 : 1;
    }
}
