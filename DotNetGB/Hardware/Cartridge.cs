using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using DotNetGB.Hardware.Cartridges;
using DotNetGB.Hardware.Cartridges.Battery;

namespace DotNetGB.Hardware
{
    public class Cartridge : IAddressSpace
    {
        public enum GameboyTypeFlag
        {
            UNIVERSAL,
            CGB,
            NON_CGB,
        }

        private static GameboyTypeFlag GetFlag(int value)
        {
            if (value == 0x80)
            {
                return GameboyTypeFlag.UNIVERSAL;
            }
            else if (value == 0xc0)
            {
                return GameboyTypeFlag.CGB;
            }
            else
            {
                return GameboyTypeFlag.NON_CGB;
            }
        }

        private readonly IAddressSpace _addressSpace;

        private readonly GameboyTypeFlag _gameboyType;

        private readonly bool _gbc;

        private readonly string _title;

        private int _dmgBootstrap;

        public Cartridge(GameboyOptions options)
        {
            FileInfo file = options.RomFile;
            int[] rom = LoadFile(file);
            var type = CartridgeTypeExtensions.GetById(rom[0x0147]);
            _title = GetTitle(rom);
            System.Diagnostics.Debug.WriteLine("Cartridge {0}, type: {1}", _title, type);
            _gameboyType = GetFlag(rom[0x0143]);
            int romBanks = GetRomBanks(rom[0x0148]);
            int ramBanks = GetRamBanks(rom[0x0149]);
            if (ramBanks == 0 && type.IsRam())
            {
                System.Diagnostics.Debug.WriteLine("RAM bank is defined to 0. Overriding to 1.");
                ramBanks = 1;
            }
            System.Diagnostics.Debug.WriteLine("ROM banks: {0}, RAM banks: {1}", romBanks, ramBanks);

            IBattery battery = new NullBattery();
            if (type.IsBattery() && options.SupportBatterySaves)
            {
                battery = new FileBattery(file.Directory, Path.GetFileNameWithoutExtension(file.Name));
            }

            if (type.IsMbc1())
            {
                _addressSpace = new Mbc1(rom, type, battery, romBanks, ramBanks);
            }
            else if (type.IsMbc2())
            {
                _addressSpace = new Mbc2(rom, type, battery, romBanks);
            }
            else if (type.IsMbc3())
            {
                _addressSpace = new Mbc3(rom, type, battery, romBanks, ramBanks);
            }
            else if (type.IsMbc5())
            {
                _addressSpace = new Mbc5(rom, type, battery, romBanks, ramBanks);
            }
            else
            {
                _addressSpace = new Rom(rom, type, romBanks, ramBanks);
            }

            _dmgBootstrap = options.UseBootstrap ? 0 : 1;
            if (options.ForceCgb)
            {
                _gbc = true;
            }
            else if (_gameboyType == GameboyTypeFlag.NON_CGB)
            {
                _gbc = false;
            }
            else if (_gameboyType == GameboyTypeFlag.CGB)
            {
                _gbc = true;
            }
            else
            { 
                // UNIVERSAL
                _gbc = !options.ForceDmg;
            }
        }

        private static string GetTitle(int[] rom)
        {
            var t = new StringBuilder();
            for (int i = 0x0134; i < 0x0143; i++)
            {
                char c = (char)rom[i];
                if (c == 0)
                {
                    break;
                }
                t.Append(c);
            }
            return t.ToString();
        }

        public string Title => _title;

        public bool IsGbc => _gbc;

        public bool Accepts(int address) => _addressSpace.Accepts(address) || address == 0xff50;

        public int this[int address]
        {
            get
            {
                if (_dmgBootstrap == 0 && !_gbc && (address >= 0x0000 && address < 0x0100))
                {
                    return BootRom.GAMEBOY_CLASSIC[address];
                }
                else if (_dmgBootstrap == 0 && _gbc && address >= 0x000 && address < 0x0100)
                {
                    return BootRom.GAMEBOY_COLOR[address];
                }
                else if (_dmgBootstrap == 0 && _gbc && address >= 0x200 && address < 0x0900)
                {
                    return BootRom.GAMEBOY_COLOR[address - 0x0100];
                }
                else if (address == 0xff50)
                {
                    return 0xff;
                }
                else
                {
                    return _addressSpace[address];
                }
            }
            set
            {
                if (address == 0xff50)
                {
                    _dmgBootstrap = 1;
                }
                else
                {
                    _addressSpace[address] = value;
                }
            }
        }

        private static int[] LoadFile(FileInfo file)
        {
            string ext = file.Extension;

            using (var inputStream = file.OpenRead())
            {
                if ("zip".Equals(ext, StringComparison.OrdinalIgnoreCase))
                {
                    using (var zis = new ZipArchive(inputStream))
                    {
                        foreach (var entry in zis.Entries)
                        {
                            string name = entry.Name;
                            string entryExt = Path.GetExtension(name);

                            if (new[] {"gb", "gbc", "rom"}.Any(e => e.Equals(entryExt, StringComparison.OrdinalIgnoreCase)))
                            {
                                using (var eStream = entry.Open())
                                {
                                    return Load(eStream, (int) entry.Length);
                                }
                            }
                        }
                    }

                    throw new InvalidOperationException("Can't find ROM file inside the zip.");
                }
                else
                {
                    return Load(inputStream, (int) inputStream.Length);
                }
            }
        }

        private static int[] Load(Stream stream, int length)
        {
            byte[] byteArray;
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                byteArray = ms.ToArray();
            }

            var intArray = new int[byteArray.Length];
            for (int i = 0; i < byteArray.Length; i++)
            {
                intArray[i] = byteArray[i] & 0xff;
            }
            return intArray;
        }

        private static int GetRomBanks(int id)
        {
            switch (id)
            {
                case 0:
                    return 2;

                case 1:
                    return 4;

                case 2:
                    return 8;

                case 3:
                    return 16;

                case 4:
                    return 32;

                case 5:
                    return 64;

                case 6:
                    return 128;

                case 7:
                    return 256;

                case 0x52:
                    return 72;

                case 0x53:
                    return 80;

                case 0x54:
                    return 96;

                default:
                    throw new ArgumentException($"Unsupported ROM size: 0x{id:x2}");
            }
        }

        private static int GetRamBanks(int id)
        {
            switch (id)
            {
                case 0:
                    return 0;

                case 1:
                    return 1;

                case 2:
                    return 1;

                case 3:
                    return 4;

                case 4:
                    return 16;

                default:
                    throw new ArgumentException($"Unsupported RAM size: 0x{id:x2}");
            }
        }
    }
}
