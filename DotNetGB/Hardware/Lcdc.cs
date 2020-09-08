using System;

namespace DotNetGB.Hardware
{
    public class Lcdc : IAddressSpace
    {
        public int Value { get; set; } = 0x91;

        public bool IsBgAndWindowDisplay => (Value & 0x01) != 0;

        public bool IsObjDisplay => (Value & 0x02) != 0;

        public int SpriteHeight => (Value & 0x04) == 0 ? 8 : 16;

        public int BgTileMapDisplay => (Value & 0x08) == 0 ? 0x9800 : 0x9c00;

        public int BgWindowTileData => (Value & 0x10) == 0 ? 0x9000 : 0x8000;

        public bool IsBgWindowTileDataSigned => (Value & 0x10) == 0;

        public bool IsWindowDisplay => (Value & 0x20) != 0;

        public int WindowTileMapDisplay => (Value & 0x40) == 0 ? 0x9800 : 0x9c00;

        public bool IsLcdEnabled => (Value & 0x80) != 0;

        public bool Accepts(int address) => address == 0xff40;

        public int this[int address]
        {
            get
            {
                if (address != 0xff40)
                {
                    throw new ArgumentOutOfRangeException(nameof(address));
                }

                return Value;
            }
            set
            {
                if (address != 0xff40)
                {
                    throw new ArgumentOutOfRangeException(nameof(address));
                }

                Value = value;
            }
        }
    }
}