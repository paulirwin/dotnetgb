using Xunit;

namespace DotNetGB.Tests.SoundTests
{
    public class LengthCounterTests : AbstractLengthCounterTest
    {
        [Fact]
        public void Test02()
        {
            Begin();
            DelayApu(3);
            ShouldBeAlmostOff();
        }

        [Fact]
        public void Test03()
        {
            Begin();
            Wchn(1, -10);
            DelayApu(9);
            ShouldBeAlmostOff();
        }

        [Fact]
        public void Test04()
        {
            Begin();
            Wchn(1, 0);
            DelayApu(maxlen - 1);
            ShouldBeAlmostOff();
        }

        [Fact]
        public void Test05()
        {
            Begin(); 
            DelayApu(1);
            Wchn(4, 0xc0);
            DelayApu(2);
            ShouldBeAlmostOff();
        }

        [Fact]
        public void Test06()
        {
            Begin();
            DelayApu(4);
            Wchn(4, 0xc0);
            DelayApu(maxlen - 1);
            ShouldBeAlmostOff();
        }

        [Fact]
        public void Test07()
        {
            Begin();
            DelayApu(4);
            Wchn(4, 0x00);
            Wchn(4, 0x80);
            Wchn(4, 0x40);
            DelayApu(maxlen - 1);
            ShouldBeAlmostOff();
        }

        [Fact]
        public void Test08()
        {
            Begin();
            DelayApu(4);
            ShouldBeOff();
            Wchn(4, 0);
            // following depends on the channel enabled flag (not part of the length counter)
            //        shouldBeOff();
        }

        [Fact]
        public void Test09()
        {
            Begin();
            Wchn(4, 0);
            DelayApu(4);
            Wchn(4, 0x40);
            DelayApu(3);
            ShouldBeAlmostOff();
        }

        [Fact]
        public void Test10()
        {
            Begin();
            DelayApu(4);
            ShouldBeOff();
            Wchn(1, -2);
            // following depends on the channel enabled flag (not part of the length counter)
            //shouldBeOff();
        }

        [Fact]
        public void Test11()
        {
            Begin();
            DelayApu(4);
            Wchn(1, -8);
            DelayApu(4);
            Wchn(4, 0xc0);
            DelayApu(3);
            ShouldBeAlmostOff();
        }

        [Fact]
        public void Test12()
        {
            Begin();
            DelayApu(4);
            Wchn(1, 0);
            DelayApu(32);
            Wchn(4, 0xc0);
            DelayApu(maxlen - 33);
            ShouldBeAlmostOff();
        }

        private void Begin()
        {
            SyncApu();
            Delay(2048);
            Wchn(4, 0x40);
            Wchn(1, -4);
            Wchn(4, 0xc0);
        }

        private void ShouldBeOn()
        {
            if (lengthCounter.IsEnabled)
            {
                Assert.NotEqual(0, lengthCounter.Value);
            }
        }

        private void ShouldBeAlmostOff()
        {
            ShouldBeOn();
            DelayApu(1);
            ShouldBeOff();
        }

        private void ShouldBeOff()
        {
            Assert.True(lengthCounter.IsEnabled && lengthCounter.Value == 0);
        }

        private void Delay(int cpuCycles)
        {
            DelayClocks(cpuCycles * 4);
        }
    }
}
