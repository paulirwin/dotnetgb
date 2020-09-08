using System;

namespace DotNetGB.Hardware.Cartridges.Rtc
{
    public class VirtualClock : IClock
    {
        private long _clock = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public long CurrentTimeMillis => _clock;

        public void Forward(TimeSpan unit)
        {
            _clock += (long)unit.TotalMilliseconds;
        }
    }
}
