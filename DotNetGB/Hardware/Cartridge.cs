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
            : this(options, LoadFile(options.RomFile))
        {
        }

        public Cartridge(GameboyOptions options, Stream romStream)
            : this(options, Load(romStream))
        {
        }

        public Cartridge(GameboyOptions options, int[] rom)
        {
            FileInfo? file = options.RomFile;
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
            if (type.IsBattery() && options.SupportBatterySaves && file != null)
            {
                battery = new FileBattery(file.Directory, Path.GetFileNameWithoutExtension(file.Name));
            }

            if (type.IsMbc1())
            {
                _addressSpace = new Mbc1(rom, battery, romBanks, ramBanks);
            }
            else if (type.IsMbc2())
            {
                _addressSpace = new Mbc2(rom, battery);
            }
            else if (type.IsMbc3())
            {
                _addressSpace = new Mbc3(rom, battery, ramBanks);
            }
            else if (type.IsMbc5())
            {
                _addressSpace = new Mbc5(rom, battery, ramBanks);
            }
            else
            {
                _addressSpace = new Rom(rom);
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

                if (_dmgBootstrap == 0 && _gbc && address >= 0x000 && address < 0x0100)
                {
                    return BootRom.GAMEBOY_COLOR[address];
                }

                if (_dmgBootstrap == 0 && _gbc && address >= 0x200 && address < 0x0900)
                {
                    return BootRom.GAMEBOY_COLOR[address - 0x0100];
                }

                return address == 0xff50 ? 0xff : _addressSpace[address];
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

            using var inputStream = file.OpenRead();

            if ("zip".Equals(ext, StringComparison.OrdinalIgnoreCase))
            {
                using var zis = new ZipArchive(inputStream);

                foreach (var entry in zis.Entries)
                {
                    string name = entry.Name;
                    string entryExt = Path.GetExtension(name);

                    if (new[] {"gb", "gbc", "rom"}.Any(e => e.Equals(entryExt, StringComparison.OrdinalIgnoreCase)))
                    {
                        using var eStream = entry.Open();

                        return Load(eStream);
                    }
                }

                throw new InvalidOperationException("Can't find ROM file inside the zip.");
            }

            return Load(inputStream);
        }

        private static int[] Load(Stream stream)
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
            return id switch
            {
                0 => 2,
                1 => 4,
                2 => 8,
                3 => 16,
                4 => 32,
                5 => 64,
                6 => 128,
                7 => 256,
                0x52 => 72,
                0x53 => 80,
                0x54 => 96,
                _ => throw new ArgumentException($"Unsupported ROM size: 0x{id:x2}")
            };
        }

        private static int GetRamBanks(int id)
        {
            return id switch
            {
                0 => 0,
                1 => 1,
                2 => 1,
                3 => 4,
                4 => 16,
                _ => throw new ArgumentException($"Unsupported RAM size: 0x{id:x2}")
            };
        }
    }
}
