namespace DotNetGB.Hardware
{
    public class TileAttributes
    {
        public static readonly TileAttributes EMPTY;

        public static readonly TileAttributes[] ATTRIBUTES;

        static TileAttributes()
        {
            ATTRIBUTES = new TileAttributes[256];
            for (int i = 0; i < 256; i++)
            {
                ATTRIBUTES[i] = new TileAttributes(i);
            }

            EMPTY = ATTRIBUTES[0];
        }

        private readonly int _value;

        private TileAttributes(int value)
        {
            _value = value;
        }

        public static TileAttributes ValueOf(int value)
        {
            return ATTRIBUTES[value];
        }

        public bool IsPriority => (_value & (1 << 7)) != 0;

        public bool IsYFlip => (_value & (1 << 6)) != 0;

        public bool IsXFlip => (_value & (1 << 5)) != 0;

        public GpuRegister DmgPalette => (_value & (1 << 4)) == 0 ? GpuRegister.OBP0 : GpuRegister.OBP1;

        public int Bank => (_value & (1 << 3)) == 0 ? 0 : 1;

        public int ColorPaletteIndex => _value & 0x07;
    }
}