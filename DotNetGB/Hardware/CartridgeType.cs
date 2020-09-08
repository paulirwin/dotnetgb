using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotNetGB.Hardware
{
    public enum CartridgeType
    {
        ROM = 0x00,
        ROM_MBC1 = 0x01,
        ROM_MBC1_RAM = 0x02,
        ROM_MBC1_RAM_BATTERY = 0x03,
        ROM_MBC2 = 0x05,
        ROM_MBC2_BATTERY = 0x06,
        ROM_RAM = 0x08,
        ROM_RAM_BATTERY = 0x09,
        ROM_MMM01 = 0x0b,
        ROM_MMM01_SRAM = 0x0c,
        ROM_MMM01_SRAM_BATTERY = 0x0d,
        ROM_MBC3_TIMER_BATTERY = 0x0f,
        ROM_MBC3_TIMER_RAM_BATTERY = 0x10,
        ROM_MBC3 = 0x11,
        ROM_MBC3_RAM = 0x12,
        ROM_MBC3_RAM_BATTERY = 0x13,
        ROM_MBC5 = 0x19,
        ROM_MBC5_RAM = 0x1a,
        ROM_MBC5_RAM_BATTERY = 0x01b,
        ROM_MBC5_RUMBLE = 0x1c,
        ROM_MBC5_RUMBLE_SRAM = 0x1d,
        ROM_MBC5_RUMBLE_SRAM_BATTERY = 0x1e,
    }

    public static class CartridgeTypeExtensions
    {
        public static bool IsMbc1(this CartridgeType type) => type.NameContainsSegment("MBC1");

        public static bool IsMbc2(this CartridgeType type) => type.NameContainsSegment("MBC2");

        public static bool IsMbc3(this CartridgeType type) => type.NameContainsSegment("MBC3");

        public static bool IsMbc5(this CartridgeType type) => type.NameContainsSegment("MBC5");

        public static bool IsMmm01(this CartridgeType type) => type.NameContainsSegment("MMM01");

        public static bool IsRam(this CartridgeType type) => type.NameContainsSegment("RAM");

        public static bool IsSram(this CartridgeType type) => type.NameContainsSegment("SRAM");
        
        public static bool IsTimer(this CartridgeType type) => type.NameContainsSegment("TIMER");

        public static bool IsBattery(this CartridgeType type) => type.NameContainsSegment("BATTERY");

        public static bool IsRumble(this CartridgeType type) => type.NameContainsSegment("RUMBLE");
        
        private static bool NameContainsSegment(this CartridgeType type, string segment)
        {
            var p = new Regex("(^|_)" + segment + "($|_)");
            return p.IsMatch(type.ToString());
        }

        public static CartridgeType GetById(int id)
        {
            foreach (var t in Enum.GetValues(typeof(CartridgeType)).OfType<CartridgeType>())
            {
                if ((int)t == id)
                {
                    return t;
                }
            }
            throw new ArgumentException($"Unsupported cartridge type: 0x{id:x2}");
        }
    }
}
