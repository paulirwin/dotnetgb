using System;
using DotNetGB.Hardware.Cartridges.Battery;

namespace DotNetGB.Hardware.Cartridges
{
    public class Mbc1 : IAddressSpace
    {
        private static readonly int[] NINTENDO_LOGO = {
            0xCE, 0xED, 0x66, 0x66, 0xCC, 0x0D, 0x00, 0x0B, 0x03, 0x73, 0x00, 0x83, 0x00, 0x0C, 0x00, 0x0D,
            0x00, 0x08, 0x11, 0x1F, 0x88, 0x89, 0x00, 0x0E, 0xDC, 0xCC, 0x6E, 0xE6, 0xDD, 0xDD, 0xD9, 0x99,
            0xBB, 0xBB, 0x67, 0x63, 0x6E, 0x0E, 0xEC, 0xCC, 0xDD, 0xDC, 0x99, 0x9F, 0xBB, 0xB9, 0x33, 0x3E
        };

        private readonly int _romBanks;

        private readonly int _ramBanks;

        private readonly int[] _cartridge;

        private readonly int[] _ram;

        private readonly IBattery _battery;

        private readonly bool _multicart;

        private int _selectedRamBank;

        private int _selectedRomBank = 1;

        private int _memoryModel;

        private bool _ramWriteEnabled;

        private int _cachedRomBankFor0x0000 = -1;

        private int _cachedRomBankFor0x4000 = -1;

        public Mbc1(int[] cartridge, IBattery battery, int romBanks, int ramBanks)
        {
            _multicart = romBanks == 64 && IsMulticart(cartridge);
            _cartridge = cartridge;
            _ramBanks = ramBanks;
            _romBanks = romBanks;
            _ram = new int[0x2000 * _ramBanks];
            for (int i = 0; i < _ram.Length; i++)
            {
                _ram[i] = 0xff;
            }

            _battery = battery;
            battery.LoadRam(_ram);
        }

        public bool Accepts(int address) => (address >= 0x0000 && address < 0x8000) ||
                                            (address >= 0xa000 && address < 0xc000);

        public int this[int address]
        {
            get
            {
                if (address >= 0x0000 && address < 0x4000)
                {
                    return GetRomByte(GetRomBankFor0x0000(), address);
                }

                if (address >= 0x4000 && address < 0x8000)
                {
                    return GetRomByte(GetRomBankFor0x4000(), address - 0x4000);
                }

                if (address >= 0xa000 && address < 0xc000)
                {
                    if (_ramWriteEnabled)
                    {
                        int ramAddress = GetRamAddress(address);
                        if (ramAddress < _ram.Length)
                        {
                            return _ram[ramAddress];
                        }

                        return 0xff;
                    }

                    return 0xff;
                }

                throw new ArgumentOutOfRangeException(address.ToString("x2"));
            }
            set
            {
                if (address >= 0x0000 && address < 0x2000)
                {
                    _ramWriteEnabled = (value & 0b1111) == 0b1010;
                    if (!_ramWriteEnabled)
                    {
                        _battery.SaveRam(_ram);
                    }
                    //Trace.WriteLine($"RAM write: {_ramWriteEnabled}");
                }
                else if (address >= 0x2000 && address < 0x4000)
                {
                    //Trace.WriteLine($"Low 5 bits of ROM bank: {(value & 0b00011111)}");
                    int bank = _selectedRomBank & 0b01100000;
                    bank |= (value & 0b00011111);
                    SelectRomBank(bank);
                    _cachedRomBankFor0x0000 = _cachedRomBankFor0x4000 = -1;
                }
                else if (address >= 0x4000 && address < 0x6000 && _memoryModel == 0)
                {
                    //Trace.WriteLine($"High 2 bits of ROM bank: {((value & 0b11) << 5)}");
                    int bank = _selectedRomBank & 0b00011111;
                    bank |= ((value & 0b11) << 5);
                    SelectRomBank(bank);
                    _cachedRomBankFor0x0000 = _cachedRomBankFor0x4000 = -1;
                }
                else if (address >= 0x4000 && address < 0x6000 && _memoryModel == 1)
                {
                    //Trace.WriteLine($"RAM bank: {(value & 0b11)}");
                    int bank = value & 0b11;
                    _selectedRamBank = bank;
                    _cachedRomBankFor0x0000 = _cachedRomBankFor0x4000 = -1;
                }
                else if (address >= 0x6000 && address < 0x8000)
                {
                    //Trace.WriteLine($"Memory mode: {(value & 1)}");
                    _memoryModel = value & 1;
                    _cachedRomBankFor0x0000 = _cachedRomBankFor0x4000 = -1;
                }
                else if (address >= 0xa000 && address < 0xc000 && _ramWriteEnabled)
                {
                    int ramAddress = GetRamAddress(address);
                    if (ramAddress < _ram.Length)
                    {
                        _ram[ramAddress] = value;
                    }
                }
            }
        }

        private void SelectRomBank(int bank)
        {
            _selectedRomBank = bank;
            //Trace.WriteLine($"Selected ROM bank: {_selectedRomBank}");
        }

        private int GetRomBankFor0x0000()
        {
            if (_cachedRomBankFor0x0000 == -1)
            {
                if (_memoryModel == 0)
                {
                    _cachedRomBankFor0x0000 = 0;
                }
                else
                {
                    int bank = (_selectedRamBank << 5);
                    if (_multicart)
                    {
                        bank >>= 1;
                    }
                    bank %= _romBanks;
                    _cachedRomBankFor0x0000 = bank;
                }
            }
            return _cachedRomBankFor0x0000;
        }

        private int GetRomBankFor0x4000()
        {
            if (_cachedRomBankFor0x4000 == -1)
            {
                int bank = _selectedRomBank;
                if (bank % 0x20 == 0)
                {
                    bank++;
                }
                if (_memoryModel == 1)
                {
                    bank &= 0b00011111;
                    bank |= (_selectedRamBank << 5);
                }
                if (_multicart)
                {
                    bank = ((bank >> 1) & 0x30) | (bank & 0x0f);
                }
                bank %= _romBanks;
                _cachedRomBankFor0x4000 = bank;
            }
            return _cachedRomBankFor0x4000;
        }

        private int GetRomByte(int bank, int address)
        {
            int cartOffset = bank * 0x4000 + address;
            if (cartOffset < _cartridge.Length)
            {
                return _cartridge[cartOffset];
            }

            return 0xff;
        }

        private int GetRamAddress(int address)
        {
            if (_memoryModel == 0)
            {
                return address - 0xa000;
            }

            return (_selectedRamBank % _ramBanks) * 0x2000 + (address - 0xa000);
        }

        private static bool IsMulticart(int[] rom)
        {
            int logoCount = 0;
            for (int i = 0; i < rom.Length; i += 0x4000)
            {
                bool logoMatches = true;
                for (int j = 0; j < NINTENDO_LOGO.Length; j++)
                {
                    if (rom[i + 0x104 + j] != NINTENDO_LOGO[j])
                    {
                        logoMatches = false;
                        break;
                    }
                }
                if (logoMatches)
                {
                    logoCount++;
                }
            }
            return logoCount > 1;
        }
    }
}
