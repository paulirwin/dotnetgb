using System;
using DotNetGB.Hardware.Cartridges.Battery;
using DotNetGB.Hardware.Cartridges.Rtc;

namespace DotNetGB.Hardware.Cartridges
{
    public class Mbc3 : IAddressSpace
    {
        private readonly CartridgeType _type;

        private readonly int _ramBanks;

        private readonly int[] _cartridge;

        private readonly int[] _ram;

        private readonly RealTimeClock _clock;

        private readonly IBattery _battery;

        private int _selectedRamBank;

        private int _selectedRomBank = 1;

        private bool _ramWriteEnabled;

        private int _latchClockReg = 0xff;

        private bool _clockLatched;

        public Mbc3(int[] cartridge, CartridgeType type, IBattery battery, int romBanks, int ramBanks)
        {
            _cartridge = cartridge;
            _ramBanks = ramBanks;
            _ram = new int[0x2000 * Math.Max(_ramBanks, 1)];
            for (int i = 0; i < _ram.Length; i++)
            {
                _ram[i] = 0xff;
            }

            _type = type;
            _clock = new RealTimeClock(new SystemClock());
            _battery = battery;

            long[] clockData = new long[12];
            battery.LoadRamWithClock(_ram, clockData);
            _clock.Deserialize(clockData);
        }

        public bool Accepts(int address) =>
            (address >= 0x0000 && address < 0x8000) ||
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
                else if (address >= 0xa000 && address < 0xc000 && _selectedRamBank < 4)
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
                else if (address >= 0xa000 && address < 0xc000 && _selectedRamBank >= 4)
                {
                    return GetTimer();
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
                        _battery.SaveRamWithClock(_ram, _clock.Serialize());
                    }
                }
                else if (address >= 0x2000 && address < 0x4000)
                {
                    int bank = value & 0b01111111;
                    SelectRomBank(bank);
                }
                else if (address >= 0x4000 && address < 0x6000)
                {
                    _selectedRamBank = value;
                }
                else if (address >= 0x6000 && address < 0x8000)
                {
                    if (value == 0x01 && _latchClockReg == 0x00)
                    {
                        if (_clockLatched)
                        {
                            _clock.Unlatch();
                            _clockLatched = false;
                        }
                        else
                        {
                            _clock.Latch();
                            _clockLatched = true;
                        }
                    }

                    _latchClockReg = value;
                }
                else if (address >= 0xa000 && address < 0xc000 && _ramWriteEnabled && _selectedRamBank < 4)
                {
                    int ramAddress = GetRamAddress(address);
                    if (ramAddress < _ram.Length)
                    {
                        _ram[ramAddress] = value;
                    }
                }
                else if (address >= 0xa000 && address < 0xc000 && _ramWriteEnabled && _selectedRamBank >= 4)
                {
                    SetTimer(value);
                }
            }
        }

        private void SelectRomBank(int bank)
        {
            if (bank == 0)
            {
                bank = 1;
            }

            _selectedRomBank = bank;
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

        private int GetTimer()
        {
            switch (_selectedRamBank)
            {
                case 0x08:
                    return _clock.Seconds;

                case 0x09:
                    return _clock.Minutes;

                case 0x0a:
                    return _clock.Hours;

                case 0x0b:
                    return _clock.DayCounter & 0xff;

                case 0x0c:
                    int result = ((_clock.DayCounter & 0x100) >> 8);
                    result |= _clock.IsHalt ? (1 << 6) : 0;
                    result |= _clock.IsCounterOverflow ? (1 << 7) : 0;
                    return result;
            }

            return 0xff;
        }

        private void SetTimer(int value)
        {
            int dayCounter = _clock.DayCounter;
            switch (_selectedRamBank)
            {
                case 0x08:
                    _clock.Seconds = value;
                    break;

                case 0x09:
                    _clock.Minutes = value;
                    break;

                case 0x0a:
                    _clock.Hours = value;
                    break;

                case 0x0b:
                    _clock.DayCounter = (dayCounter & 0x100) | (value & 0xff);
                    break;

                case 0x0c:
                    _clock.DayCounter = (dayCounter & 0xff) | ((value & 1) << 8);
                    _clock.IsHalt = (value & (1 << 6)) != 0;
                    if ((value & (1 << 7)) == 0)
                    {
                        _clock.ClearCounterOverflow();
                    }

                    break;
            }
        }
    }
}