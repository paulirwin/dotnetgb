namespace DotNetGB.Hardware.Cartridges
{
    public class Rom : IAddressSpace
    {
        private readonly int[] _rom;

        public Rom(int[] rom)
        {
            _rom = rom;
        }

        public bool Accepts(int address) => (address >= 0x0000 && address < 0x8000) ||
                                            (address >= 0xa000 && address < 0xc000);

        public int this[int address]
        {
            get
            {
                if (address >= 0x0000 && address < 0x8000)
                {
                    return _rom[address];
                }

                return 0;
            }
            set { }
        }
    }
}
