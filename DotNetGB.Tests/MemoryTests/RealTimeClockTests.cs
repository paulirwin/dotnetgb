using System;
using DotNetGB.Hardware.Cartridges.Rtc;
using Xunit;

namespace DotNetGB.Tests.MemoryTests
{
    public class RealTimeClockTests
    {
        private readonly RealTimeClock rtc;
        private readonly VirtualClock clock;

        public RealTimeClockTests()
        {
            clock = new VirtualClock();
            rtc = new RealTimeClock(clock);
        }

        [Fact]
        public void TestBasicGet()
        {
            Forward(5, 8, 12, 2);
            AssertClockEquals(5, 8, 12, 2);
        }

        [Fact]
        public void TestLatch()
        {
            Forward(5, 8, 12, 2);

            rtc.Latch();
            Forward(10, 5, 19, 4);
            AssertClockEquals(5, 8, 12, 2);
            rtc.Unlatch();

            AssertClockEquals(5 + 10, 8 + 5, 12 + 19, 2 + 4);
        }

        [Fact]
        public void TestCounterOverflow()
        {
            Forward(511, 23, 59, 59);
            Assert.False(rtc.IsCounterOverflow);

            clock.Forward(TimeSpan.FromSeconds(1));
            AssertClockEquals(0, 0, 0, 0);
            Assert.True(rtc.IsCounterOverflow);

            Forward(10, 5, 19, 4);
            AssertClockEquals(10, 5, 19, 4);
            Assert.True(rtc.IsCounterOverflow);

            rtc.ClearCounterOverflow();
            AssertClockEquals(10, 5, 19, 4);
            Assert.False(rtc.IsCounterOverflow);
        }

        [Fact]
        public void SetClock()
        {
            Forward(10, 5, 19, 4);
            AssertClockEquals(10, 5, 19, 4);

            rtc.IsHalt = true;
            Assert.True(rtc.IsHalt);

            rtc.DayCounter = 10;
            rtc.Hours = 16;
            rtc.Minutes = 21;
            rtc.Seconds = 32;
            Forward(1, 1, 1, 1); // should be ignored after unhalt
            rtc.IsHalt = false;

            Assert.False(rtc.IsHalt);
            AssertClockEquals(10, 16, 21, 32);
            Forward(2, 2, 2, 2);
            AssertClockEquals(12, 18, 23, 34);
        }

        private void Forward(int days, int hours, int minutes, int seconds)
        {
            clock.Forward(TimeSpan.FromDays(days));
            clock.Forward(TimeSpan.FromHours(hours));
            clock.Forward(TimeSpan.FromMinutes(minutes));
            clock.Forward(TimeSpan.FromSeconds(seconds));
        }

        private void AssertClockEquals(int days, int hours, int minutes, int seconds)
        {
            Assert.Equal(days, rtc.DayCounter);
            Assert.Equal(hours, rtc.Hours);
            Assert.Equal(minutes, rtc.Minutes);
            Assert.Equal(seconds, rtc.Seconds);
        }
    }
}
