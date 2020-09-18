using System.Collections.Generic;
using System.Linq;
using DotNetGB.Hardware;
using DotNetGB.Hardware.GpuPhases;
using Xunit;

namespace DotNetGB.Tests.GpuTests
{
    public class PixelFifoTests
    {
        private readonly DmgPixelFifo fifo;

        public PixelFifoTests()
        {
            var r = new MemoryRegisters(GpuRegister.Values.OfType<MemoryRegisters.IRegister>());
            r.Put(GpuRegister.BGP, 0b11100100);
            fifo = new DmgPixelFifo(new NullDisplay(), new Lcdc(), r);
        }

        [Fact]
        public void TestEnqueue()
        {
            fifo.Enqueue8Pixels(Zip(0b11001001, 0b11110000, false), TileAttributes.EMPTY);
            Assert.Equal(new[] { 3, 3, 2, 2, 1, 0, 0, 1 }, ArrayQueueAsList(fifo.Pixels));
        }

        [Fact]
        public void TestDequeue()
        {
            fifo.Enqueue8Pixels(Zip(0b11001001, 0b11110000, false), TileAttributes.EMPTY);
            fifo.Enqueue8Pixels(Zip(0b10101011, 0b11100111, false), TileAttributes.EMPTY);
            Assert.Equal(0b11, fifo.DequeuePixel());
            Assert.Equal(0b11, fifo.DequeuePixel());
            Assert.Equal(0b10, fifo.DequeuePixel());
            Assert.Equal(0b10, fifo.DequeuePixel());
            Assert.Equal(0b01, fifo.DequeuePixel());
        }

        [Fact]
        public void TestZip()
        {
            Assert.Equal(new[] { 3, 3, 2, 2, 1, 0, 0, 1 }, Zip(0b11001001, 0b11110000, false));
            Assert.Equal(new[] { 1, 0, 0, 1, 2, 2, 3, 3 }, Zip(0b11001001, 0b11110000, true));
        }

        private static int[] Zip(int data1, int data2, bool reverse)
        {
            return Fetcher.Zip(data1, data2, reverse, new int[8]);
        }

        private static IList<int> ArrayQueueAsList(IntQueue queue)
        {
            var l = new List<int>(queue.Size);
            for (int i = 0; i < queue.Size; i++)
            {
                l.Add(queue.Get(i));
            }
            return l;
        }
    }
}
