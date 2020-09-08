using SpritePosition = DotNetGB.Hardware.GpuPhases.OamSearch.SpritePosition;
using static DotNetGB.Hardware.GpuRegister;

namespace DotNetGB.Hardware.GpuPhases
{
    public class PixelTransfer : IGpuPhase
    {
        private readonly IPixelFifo _fifo;

        private readonly Fetcher _fetcher;

        private readonly IDisplay _display;

        private readonly MemoryRegisters _r;

        private readonly Lcdc _lcdc;

        private readonly bool _gbc;

        private SpritePosition?[] _sprites;

        private int _droppedPixels;

        private int _x;

        private bool _window;

        public PixelTransfer(IAddressSpace videoRam0, IAddressSpace? videoRam1, IAddressSpace oemRam, IDisplay display, Lcdc lcdc, MemoryRegisters r, bool gbc, ColorPalette bgPalette, ColorPalette oamPalette)
        {
            _r = r;
            _lcdc = lcdc;
            _gbc = gbc;
            if (gbc)
            {
                _fifo = new ColorPixelFifo(lcdc, display, bgPalette, oamPalette);
            }
            else
            {
                _fifo = new DmgPixelFifo(display, lcdc, r);
            }
            _fetcher = new Fetcher(_fifo, videoRam0, videoRam1, oemRam, lcdc, r, gbc);
            _display = display;
        }

        public PixelTransfer Start(SpritePosition?[] sprites)
        {
            _sprites = sprites;
            _droppedPixels = 0;
            _x = 0;
            _window = false;

            _fetcher.Init();
            if (_gbc || _lcdc.IsBgAndWindowDisplay)
            {
                StartFetchingBackground();
            }
            else
            {
                _fetcher.FetchingDisabled();
            }
            return this;
        }

        public bool Tick()
        {
            _fetcher.Tick();
            if (_lcdc.IsBgAndWindowDisplay || _gbc)
            {
                if (_fifo.Length <= 8)
                {
                    return true;
                }
                if (_droppedPixels < _r.Get(SCX) % 8)
                {
                    _fifo.DropPixel();
                    _droppedPixels++;
                    return true;
                }
                if (!_window && _lcdc.IsWindowDisplay && _r.Get(LY) >= _r.Get(WY) && _x == _r.Get(WX) - 7)
                {
                    _window = true;
                    StartFetchingWindow();
                    return true;
                }
            }

            if (_lcdc.IsObjDisplay)
            {
                if (_fetcher.SpriteInProgress)
                {
                    return true;
                }
                bool spriteAdded = false;
                for (int i = 0; i < _sprites.Length; i++)
                {
                    SpritePosition s = _sprites[i];
                    if (s == null)
                    {
                        continue;
                    }
                    if (_x == 0 && s.X < 8)
                    {
                        if (!spriteAdded)
                        {
                            _fetcher.AddSprite(s, 8 - s.X, i);
                            spriteAdded = true;
                        }
                        _sprites[i] = null;
                    }
                    else if (s.X - 8 == _x)
                    {
                        if (!spriteAdded)
                        {
                            _fetcher.AddSprite(s, 0, i);
                            spriteAdded = true;
                        }
                        _sprites[i] = null;
                    }
                    if (spriteAdded)
                    {
                        return true;
                    }
                }
            }

            _fifo.PutPixelToScreen();
            if (++_x == 160)
            {
                return false;
            }
            return true;
        }

        private void StartFetchingBackground()
        {
            int bgX = _r.Get(SCX) / 0x08;
            int bgY = (_r.Get(SCY) + _r.Get(LY)) % 0x100;

            _fetcher.StartFetching(_lcdc.BgTileMapDisplay + (bgY / 0x08) * 0x20, _lcdc.BgWindowTileData, bgX, _lcdc.IsBgWindowTileDataSigned, bgY % 0x08);
        }

        private void StartFetchingWindow()
        {
            int winX = (_x - _r.Get(WX) + 7) / 0x08;
            int winY = _r.Get(LY) - _r.Get(WY);

            _fetcher.StartFetching(_lcdc.WindowTileMapDisplay + (winY / 0x08) * 0x20, _lcdc.BgWindowTileData, winX, _lcdc.IsBgWindowTileDataSigned, winY % 0x08);
        }
    }
}
