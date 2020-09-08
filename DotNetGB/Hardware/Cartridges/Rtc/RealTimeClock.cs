namespace DotNetGB.Hardware.Cartridges.Rtc
{
    public class RealTimeClock
    {
        private readonly IClock _clock;

        private long _offsetSec;

        private long _clockStart;

        private bool _halt;

        private long _latchStart;

        private int _haltSeconds;

        private int _haltMinutes;

        private int _haltHours;

        private int _haltDays;

        public RealTimeClock(IClock clock)
        {
            _clock = clock;
            _clockStart = clock.CurrentTimeMillis;
        }

        public void Latch()
        {
            _latchStart = _clock.CurrentTimeMillis;
        }

        public void Unlatch()
        {
            _latchStart = 0;
        }

        public int Seconds
        {
            get => (int) (ClockTimeInSec() % 60);
            set
            {
                if (!_halt)
                {
                    return;
                }

                _haltSeconds = value;
            }
        }

        public int Minutes
        {
            get => (int)((ClockTimeInSec() % (60 * 60)) / 60);
            set
            {
                if (!_halt)
                {
                    return;
                }

                _haltMinutes = value;
            }
        }

        public int Hours
        {
            get => (int)((ClockTimeInSec() % (60 * 60 * 24)) / (60 * 60));
            set
            {
                if (!_halt)
                {
                    return;
                }

                _haltHours = value;
            }
        }

        public int DayCounter
        {
            get => (int)(ClockTimeInSec() % (60 * 60 * 24 * 512) / (60 * 60 * 24));
            set
            {
                if (!_halt)
                {
                    return;
                }

                _haltDays = value;
            }
        }

        public bool IsHalt
        {
            get => _halt;
            set
            {
                if (value && !_halt)
                {
                    Latch();
                    _haltSeconds = Seconds;
                    _haltMinutes = Minutes;
                    _haltHours = Hours;
                    _haltDays = DayCounter;
                    Unlatch();
                }
                else if (!value && _halt)
                {
                    _offsetSec = _haltSeconds + _haltMinutes * 60 + _haltHours * 60 * 60 + _haltDays * 60 * 60 * 24;
                    _clockStart = _clock.CurrentTimeMillis;
                }
                _halt = value;
            }
        }

        public bool IsCounterOverflow => ClockTimeInSec() >= 60 * 60 * 24 * 512;

        public void ClearCounterOverflow()
        {
            while (IsCounterOverflow)
            {
                _offsetSec -= 60 * 60 * 24 * 512;
            }
        }

        private long ClockTimeInSec()
        {
            long now;
            if (_latchStart == 0)
            {
                now = _clock.CurrentTimeMillis;
            }
            else
            {
                now = _latchStart;
            }

            return (now - _clockStart) / 1000 + _offsetSec;
        }

        public void Deserialize(long[] clockData)
        {
            long seconds = clockData[0];
            long minutes = clockData[1];
            long hours = clockData[2];
            long days = clockData[3];
            long daysHigh = clockData[4];
            long timestamp = clockData[10];

            _clockStart = timestamp * 1000;
            _offsetSec = seconds + minutes * 60 + hours * 60 * 60 + days * 24 * 60 * 60 + daysHigh * 256 * 24 * 60 * 60;
        }

        public long[] Serialize()
        {
            long[] clockData = new long[11];
            Latch();
            clockData[0] = clockData[5] = Seconds;
            clockData[1] = clockData[6] = Minutes;
            clockData[2] = clockData[7] = Hours;
            clockData[3] = clockData[8] = DayCounter % 256;
            clockData[4] = clockData[9] = DayCounter / 256;
            clockData[10] = _latchStart / 1000;
            Unlatch();
            return clockData;
        }
    }
}
