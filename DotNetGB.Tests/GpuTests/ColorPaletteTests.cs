using DotNetGB.Hardware;
using Xunit;

namespace DotNetGB.Tests.GpuTests
{
    public class ColorPaletteTests
    {
        [Fact]
        public void TestAutoIncrement()
        {
            var p = new ColorPalette(0xff68)
            {
                [0xff68] = 0x80,
                [0xff69] = 0x00,
                [0xff69] = 0xaa,
                [0xff69] = 0x11,
                [0xff69] = 0xbb,
                [0xff69] = 0x22,
                [0xff69] = 0xcc,
                [0xff69] = 0x33,
                [0xff69] = 0xdd,
                [0xff69] = 0x44,
                [0xff69] = 0xee,
                [0xff69] = 0x55,
                [0xff69] = 0xff,
            };
            Assert.Equal(new[] { 0xaa00, 0xbb11, 0xcc22, 0xdd33 }, p.GetPalette(0));
            Assert.Equal(new[] { 0xee44, 0xff55, 0x0000, 0x0000 }, p.GetPalette(1));
        }
    }
}
