using System;

namespace DotNetGB.Hardware
{
    public class UndocumentedGbcRegisters : IAddressSpace
    {
        private readonly Ram _ram = new Ram(0xff72, 6);

        private int _xff6c;

        public UndocumentedGbcRegisters()
        {
            _xff6c = 0xfe;
            _ram[0xff74] = 0xff;
            _ram[0xff75] = 0x8f;
        }

        public bool Accepts(int address) => address == 0xff6c || _ram.Accepts(address);

        public int this[int address]
        {
            get
            {
                if (address == 0xff6c)
                {
                    return _xff6c;
                }

                if (_ram.Accepts(address))
                {
                    return _ram[address];
                }

                throw new ArgumentOutOfRangeException();
            }
            set
            {
                switch (address)
                {
                    case 0xff6c:
                        _xff6c = 0xfe | (value & 1);
                        break;

                    case 0xff72:
                    case 0xff73:
                    case 0xff74:
                        _ram[address] = value;
                        break;

                    case 0xff75:
                        _ram[address] = 0x8f | (value & 0b01110000);
                        break;
                }
            }
        }
    }
}
