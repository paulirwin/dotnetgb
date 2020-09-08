using System;

namespace DotNetGB.Hardware.Cartridges.Rtc
{
    public class SystemClock : IClock
    {
        public long CurrentTimeMillis => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
