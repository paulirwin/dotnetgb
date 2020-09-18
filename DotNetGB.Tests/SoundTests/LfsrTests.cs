using DotNetGB.Hardware.Sounds;
using Xunit;

namespace DotNetGB.Tests.SoundTests
{
    public class LfsrTests
    {
        [Fact]
        public void TestLfsr()
        {
            var lfsr = new Lfsr();
            int previousValue = 0;
            for (int i = 0; i < 100; i++)
            {
                lfsr.NextBit(false);
                Assert.NotEqual(previousValue, lfsr.Value);
                previousValue = lfsr.Value;
            }
        }

        [Fact]
        public void TestLfsrWidth7()
        {
            var lfsr = new Lfsr();
            int previousValue = 0;
            for (int i = 0; i < 100; i++)
            {
                lfsr.NextBit(true);
                Assert.NotEqual(previousValue, lfsr.Value);
                previousValue = lfsr.Value;
            }
        }
    }
}
