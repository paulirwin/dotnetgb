using System.Linq;
using SpritePosition = DotNetGB.Hardware.GpuPhases.OamSearch.SpritePosition;
using static DotNetGB.Hardware.GpuRegister;
using static DotNetGB.Util.BitUtils;

namespace DotNetGB.Hardware.GpuPhases
{
    public class Fetcher
    {
        private enum State
        {
            READ_TILE_ID, READ_DATA_1, READ_DATA_2, PUSH,
            READ_SPRITE_TILE_ID, READ_SPRITE_FLAGS, READ_SPRITE_DATA_1, READ_SPRITE_DATA_2, PUSH_SPRITE,
        }

        private static readonly State[] IN_PROGRESS_STATES =
        {
            State.READ_SPRITE_TILE_ID, 
            State.READ_SPRITE_FLAGS, 
            State.READ_SPRITE_DATA_1, 
            State.READ_SPRITE_DATA_2, 
            State.PUSH_SPRITE,
        };

        private static readonly int[] EMPTY_PIXEL_LINE = new int[8];

        private readonly IPixelFifo _fifo;

        private readonly IAddressSpace _videoRam0;

        private readonly IAddressSpace? _videoRam1;

        private readonly IAddressSpace _oemRam;

        private readonly MemoryRegisters _r;

        private readonly Lcdc _lcdc;

        private readonly bool _gbc;

        private readonly int[] _pixelLine = new int[8];

        private State _state;

        private bool _fetchingDisabled;

        private int _mapAddress;

        private int _xOffset;

        private int _tileDataAddress;

        private bool _tileIdSigned;

        private int _tileLine;

        private int _tileId;

        private TileAttributes _tileAttributes = TileAttributes.EMPTY;

        private int _tileData1;

        private int _tileData2;

        private int _spriteTileLine;

        private SpritePosition? _sprite;

        private TileAttributes? _spriteAttributes;

        private int _spriteOffset;

        private int _spriteOamIndex;

        private int _divider = 2;

        public Fetcher(IPixelFifo fifo, IAddressSpace videoRam0, IAddressSpace? videoRam1, IAddressSpace oemRam, Lcdc lcdc, MemoryRegisters registers, bool gbc)
        {
            _gbc = gbc;
            _fifo = fifo;
            _videoRam0 = videoRam0;
            _videoRam1 = videoRam1;
            _oemRam = oemRam;
            _r = registers;
            _lcdc = lcdc;
        }

        public void Init()
        {
            _state = State.READ_TILE_ID;
            _tileId = 0;
            _tileData1 = 0;
            _tileData2 = 0;
            _divider = 2;
            _fetchingDisabled = false;
        }

        public void StartFetching(int mapAddress, int tileDataAddress, int xOffset, bool tileIdSigned, int tileLine)
        {
            _mapAddress = mapAddress;
            _tileDataAddress = tileDataAddress;
            _xOffset = xOffset;
            _tileIdSigned = tileIdSigned;
            _tileLine = tileLine;
            _fifo.Clear();

            _state = State.READ_TILE_ID;
            _tileId = 0;
            _tileData1 = 0;
            _tileData2 = 0;
            _divider = 2;
        }

        public void FetchingDisabled()
        {
            _fetchingDisabled = true;
        }

        public void AddSprite(SpritePosition sprite, int offset, int oamIndex)
        {
            _sprite = sprite;
            _state = State.READ_SPRITE_TILE_ID;
            _spriteTileLine = _r.Get(LY) + 16 - sprite.Y;
            _spriteOffset = offset;
            _spriteOamIndex = oamIndex;
        }

        public void Tick()
        {
            if (_fetchingDisabled && _state == State.READ_TILE_ID)
            {
                if (_fifo.Length <= 8)
                {
                    _fifo.Enqueue8Pixels(EMPTY_PIXEL_LINE, _tileAttributes);
                }
                return;
            }

            if (--_divider == 0)
            {
                _divider = 2;
            }
            else
            {
                return;
            }

            switch (_state)
            {
                case State.READ_TILE_ID:
                    _tileId = _videoRam0[_mapAddress + _xOffset];
                    _tileAttributes = _gbc 
                        ? TileAttributes.ValueOf(_videoRam1[_mapAddress + _xOffset]) 
                        : TileAttributes.EMPTY;
                    _state = State.READ_DATA_1;
                    break;

                case State.READ_DATA_1:
                    _tileData1 = GetTileData(_tileId, _tileLine, 0, _tileDataAddress, _tileIdSigned, _tileAttributes, 8);
                    _state = State.READ_DATA_2;
                    break;

                case State.READ_DATA_2:
                    _tileData2 = GetTileData(_tileId, _tileLine, 1, _tileDataAddress, _tileIdSigned, _tileAttributes, 8);
                    _state = State.PUSH;

                    // HACK: copied from case below due to fallthrough allowed in Java
                    if (_fifo.Length <= 8)
                    {
                        _fifo.Enqueue8Pixels(Zip(_tileData1, _tileData2, _tileAttributes.IsXFlip), _tileAttributes);
                        _xOffset = (_xOffset + 1) % 0x20;
                        _state = State.READ_TILE_ID;
                    }

                    break;

                case State.PUSH:
                    if (_fifo.Length <= 8)
                    {
                        _fifo.Enqueue8Pixels(Zip(_tileData1, _tileData2, _tileAttributes.IsXFlip), _tileAttributes);
                        _xOffset = (_xOffset + 1) % 0x20;
                        _state = State.READ_TILE_ID;
                    }
                    break;

                case State.READ_SPRITE_TILE_ID:
                    _tileId = _oemRam[_sprite.Address + 2];
                    _state = State.READ_SPRITE_FLAGS;
                    break;

                case State.READ_SPRITE_FLAGS:
                    _spriteAttributes = TileAttributes.ValueOf(_oemRam[_sprite.Address + 3]);
                    _state = State.READ_SPRITE_DATA_1;
                    break;

                case State.READ_SPRITE_DATA_1:
                    if (_lcdc.SpriteHeight == 16)
                    {
                        _tileId &= 0xfe;
                    }
                    _tileData1 = GetTileData(_tileId, _spriteTileLine, 0, 0x8000, false, _spriteAttributes, _lcdc.SpriteHeight);
                    _state = State.READ_SPRITE_DATA_2;
                    break;

                case State.READ_SPRITE_DATA_2:
                    _tileData2 = GetTileData(_tileId, _spriteTileLine, 1, 0x8000, false, _spriteAttributes, _lcdc.SpriteHeight);
                    _state = State.PUSH_SPRITE;
                    break;

                case State.PUSH_SPRITE:
                    _fifo.SetOverlay(Zip(_tileData1, _tileData2, _spriteAttributes.IsXFlip), _spriteOffset, _spriteAttributes, _spriteOamIndex);
                    _state = State.READ_TILE_ID;
                    break;
            }
        }

        private int GetTileData(int tileId, int line, int byteNumber, int tileDataAddress, bool signed, TileAttributes attr, int tileHeight)
        {
            int effectiveLine;
            if (attr.IsYFlip)
            {
                effectiveLine = tileHeight - 1 - line;
            }
            else
            {
                effectiveLine = line;
            }

            int tileAddress;
            if (signed)
            {
                tileAddress = tileDataAddress + ToSigned(tileId) * 0x10;
            }
            else
            {
                tileAddress = tileDataAddress + tileId * 0x10;
            }
            var videoRam = (attr.Bank == 0 || !_gbc) ? _videoRam0 : _videoRam1;
            return videoRam[tileAddress + effectiveLine * 2 + byteNumber];
        }

        public bool SpriteInProgress => IN_PROGRESS_STATES.Contains(_state);

        public int[] Zip(int data1, int data2, bool reverse) => Zip(data1, data2, reverse, _pixelLine);

        public static int[] Zip(int data1, int data2, bool reverse, int[] pixelLine)
        {
            for (int i = 7; i >= 0; i--)
            {
                int mask = (1 << i);
                int p = 2 * ((data2 & mask) == 0 ? 0 : 1) + ((data1 & mask) == 0 ? 0 : 1);
                if (reverse)
                {
                    pixelLine[i] = p;
                }
                else
                {
                    pixelLine[7 - i] = p;
                }
            }
            return pixelLine;
        }
    }
}