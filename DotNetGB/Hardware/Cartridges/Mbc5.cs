using System;
using DotNetGB.Hardware.Cartridges.Battery;

namespace DotNetGB.Hardware.Cartridges
{
    public class Mbc5 : IAddressSpace
    {
        private readonly CartridgeType _type;

        private readonly int _romBanks;

        private readonly int _ramBanks;

        private readonly int[] _cartridge;

        private readonly int[] _ram;

        private readonly IBattery _battery;

        private int _selectedRamBank;

        private int _selectedRomBank = 1;

        private bool _ramWriteEnabled;

        public Mbc5(int[] cartridge, CartridgeType type, IBattery battery, int romBanks, int ramBanks)
        {
            _cartridge = cartridge;
            _ramBanks = ramBanks;
            _romBanks = romBanks;
            _ram = new int[0x2000 * Math.Max(_ramBanks, 1)];
            for (int i = 0; i < _ram.Length; i++)
            {
                _ram[i] = 0xff;
            }
            _type = type;
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
                    return GetRomByte(0, address);
                }
                else if (address >= 0x4000 && address < 0x8000)
                {
                    return GetRomByte(_selectedRomBank, address - 0x4000);
                }
                else if (address >= 0xa000 && address < 0xc000)
                {
                    int ramAddress = GetRamAddress(address);
                    if (ramAddress < _ram.Length)
                    {
                        return _ram[ramAddress];
                    }
                    else
                    {
                        return 0xff;
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException(address.ToString("x2"));
                }
            }
            set
            {
                if (address >= 0x0000 && address < 0x2000)
                {
                    _ramWriteEnabled = (value & 0b1010) != 0;
                    if (!_ramWriteEnabled)
                    {
                        _battery.SaveRam(_ram);
                    }
                }
                else if (address >= 0x2000 && address < 0x3000)
                {
                    _selectedRomBank = (_selectedRomBank & 0x100) | value;
                }
                else if (address >= 0x3000 && address < 0x4000)
                {
                    _selectedRomBank = (_selectedRomBank & 0x0ff) | ((value & 1) << 8);
                }
                else if (address >= 0x4000 && address < 0x6000)
                {
                    int bank = value & 0x0f;
                    if (bank < _ramBanks)
                    {
                        _selectedRamBank = bank;
                    }
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

        private int GetRomByte(int bank, int address)
        {
            int cartOffset = bank * 0x4000 + address;
            if (cartOffset < _cartridge.Length)
            {
                return _cartridge[cartOffset];
            }
            else
            {
                return 0xff;
            }
        }

        private int GetRamAddress(int address)
        {
            return _selectedRamBank * 0x2000 + (address - 0xa000);
        }
    }
}
