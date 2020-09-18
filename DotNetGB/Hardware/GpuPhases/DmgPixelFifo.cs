namespace DotNetGB.Hardware.GpuPhases
{
    public class DmgPixelFifo : IPixelFifo
    {
        private readonly IntQueue _pixels = new IntQueue(16);

        private readonly IntQueue _palettes = new IntQueue(16);

        private readonly IntQueue _pixelType = new IntQueue(16); // 0 - bg, 1 - sprite

        private readonly IDisplay _display;

        private readonly Lcdc _lcdc;

        private readonly MemoryRegisters _registers;

        public DmgPixelFifo(IDisplay display, Lcdc lcdc, MemoryRegisters registers)
        {
            _lcdc = lcdc;
            _display = display;
            _registers = registers;
        }

        public int Length => _pixels.Size;

        public void PutPixelToScreen() => _display.PutDmgPixel(DequeuePixel());

        public void DropPixel() => DequeuePixel();

        internal int DequeuePixel()
        {
            _pixelType.Dequeue();
            return GetColor(_palettes.Dequeue(), _pixels.Dequeue());
        }

        public void Enqueue8Pixels(int[] pixelLine, TileAttributes tileAttributes)
        {
            foreach (int p in pixelLine)
            {
                _pixels.Enqueue(p);
                _palettes.Enqueue(_registers.Get(GpuRegister.BGP));
                _pixelType.Enqueue(0);
            }
        }

        public void SetOverlay(int[] pixelLine, int offset, TileAttributes flags, int oamIndex)
        {
            bool priority = flags.IsPriority;
            int overlayPalette = _registers.Get(flags.DmgPalette);

            for (int j = offset; j < pixelLine.Length; j++)
            {
                int p = pixelLine[j];
                int i = j - offset;
                if (_pixelType.Get(i) == 1)
                {
                    continue;
                }
                if ((priority && _pixels.Get(i) == 0) || !priority && p != 0)
                {
                    _pixels.Set(i, p);
                    _palettes.Set(i, overlayPalette);
                    _pixelType.Set(i, 1);
                }
            }
        }

        internal IntQueue Pixels => _pixels;

        private static int GetColor(int palette, int colorIndex) => 0b11 & (palette >> (colorIndex * 2));

        public void Clear()
        {
            _pixels.Clear();
            _palettes.Clear();
            _pixelType.Clear();
        }
    }
}