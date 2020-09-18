namespace DotNetGB.Hardware.Sounds
{
    public class Lfsr
    {
        private int lfsr;

        public Lfsr()
        {
            Reset();
        }

        public void Start()
        {
            Reset();
        }

        public void Reset()
        {
            lfsr = 0x7fff;
        }

        public int NextBit(bool widthMode7)
        {
            bool x = ((lfsr & 1) ^ ((lfsr & 2) >> 1)) != 0;
            lfsr >>= 1;
            lfsr |= (x ? (1 << 14) : 0);
            if (widthMode7)
            {
                lfsr |= (x ? (1 << 6) : 0);
            }
            return 1 & ~lfsr;
        }

        internal int Value => lfsr;
    }
}
