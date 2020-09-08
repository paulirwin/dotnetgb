namespace DotNetGB.Hardware.GpuPhases
{
    public class ColorPixelFifo : IPixelFifo
    {
        private readonly IntQueue _pixels = new IntQueue(16);

        private readonly IntQueue _palettes = new IntQueue(16);

        private readonly IntQueue _priorities = new IntQueue(16);

        private readonly Lcdc _lcdc;

        private readonly IDisplay _display;

        private readonly ColorPalette _bgPalette;

        private readonly ColorPalette _oamPalette;

        public ColorPixelFifo(Lcdc lcdc, IDisplay display, ColorPalette bgPalette, ColorPalette oamPalette)
        {
            _lcdc = lcdc;
            _display = display;
            _bgPalette = bgPalette;
            _oamPalette = oamPalette;
        }


        public int Length => _pixels.Size;

        public void PutPixelToScreen() => _display.PutColorPixel(DequeuePixel());

        private int DequeuePixel() => GetColor(_priorities.Dequeue(), _palettes.Dequeue(), _pixels.Dequeue());

        public void DropPixel() => DequeuePixel();

        public void Enqueue8Pixels(int[] pixelLine, TileAttributes tileAttributes)
        {
            foreach (int p in pixelLine)
            {
                _pixels.Enqueue(p);
                _palettes.Enqueue(tileAttributes.ColorPaletteIndex);
                _priorities.Enqueue(tileAttributes.IsPriority ? 100 : -1);
            }
        }

        public void SetOverlay(int[] pixelLine, int offset, TileAttributes spriteAttr, int oamIndex)
        {
            for (int j = offset; j < pixelLine.Length; j++)
            {
                int p = pixelLine[j];
                int i = j - offset;
                if (p == 0)
                {
                    continue; // color 0 is always transparent
                }
                int oldPriority = _priorities.Get(i);

                bool put = false;
                if ((oldPriority == -1 || oldPriority == 100) && !_lcdc.IsBgAndWindowDisplay)
                { 
                    // this one takes precedence
                    put = true;
                }
                else if (oldPriority == 100)
                { 
                    // bg with priority
                    put = _pixels.Get(i) == 0;
                }
                else if (oldPriority == -1 && !spriteAttr.IsPriority)
                { 
                    // bg without priority
                    put = true;
                }
                else if (oldPriority == -1 && spriteAttr.IsPriority && _pixels.Get(i) == 0)
                { 
                    // bg without priority
                    put = true;
                }
                else if (oldPriority >= 0 && oldPriority < 10)
                { 
                    // other sprite
                    put = oldPriority > oamIndex;
                }

                if (put)
                {
                    _pixels.Set(i, p);
                    _palettes.Set(i, spriteAttr.ColorPaletteIndex);
                    _priorities.Set(i, oamIndex);
                }
            }
        }

        public void Clear()
        {
            _pixels.Clear();
            _palettes.Clear();
            _priorities.Clear();
        }

        private int GetColor(int priority, int palette, int color)
        {
            if (priority >= 0 && priority < 10)
            {
                return _oamPalette.GetPalette(palette)[color];
            }
            else
            {
                return _bgPalette.GetPalette(palette)[color];
            }
        }
    }
}