using System;

namespace DotNetGB.Hardware
{
    public class GbcRam : IAddressSpace
    {
        private readonly int[] _ram = new int[7 * 0x1000];

        private int _svbk;

        public bool Accepts(int address) => address == 0xff70 || (address >= 0xd000 && address < 0xe000);

        public int this[int address]
        {
            get => address == 0xff70 ? _svbk : _ram[Translate(address)];
            set
            {
                if (address == 0xff70)
                {
                    _svbk = value;
                }
                else
                {
                    _ram[Translate(address)] = value;
                }
            }
        }

        private int Translate(int address)
        {
            int ramBank = _svbk & 0x7;
            if (ramBank == 0)
            {
                ramBank = 1;
            }
            int result = address - 0xd000 + (ramBank - 1) * 0x1000;
            if (result < 0 || result >= _ram.Length)
            {
                throw new ArgumentOutOfRangeException();
            }
            return result;
        }
    }
}
