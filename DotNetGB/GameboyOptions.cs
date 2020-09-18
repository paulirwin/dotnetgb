using System;
using System.Collections.Generic;
using System.IO;

namespace DotNetGB
{
    public class GameboyOptions
    {
        public GameboyOptions(FileInfo romFile)
            : this(romFile, new List<string>(), new List<string>())
        {
        }

        public GameboyOptions(FileInfo romFile, ICollection<string> param, ICollection<string> shortParam)
        {
            RomFile = romFile;
            ForceDmg = param.Contains("force-dmg") || shortParam.Contains("d");
            ForceCgb = param.Contains("force-cgb") || shortParam.Contains("c");
            if (ForceDmg && ForceCgb)
            {
                throw new ArgumentException("force-dmg and force-cgb options are can't be used together");
            }
            UseBootstrap = param.Contains("use-bootstrap") || shortParam.Contains("b");
            DisableBatterySaves = param.Contains("disable-battery-saves") || shortParam.Contains("db");
            Debug = param.Contains("debug");
            Headless = param.Contains("headless");
        }

        public FileInfo RomFile { get; }

        public bool ForceDmg { get; set; }

        public bool ForceCgb { get; set; }

        public bool UseBootstrap { get; set; }

        public bool DisableBatterySaves { get; set; }

        public bool SupportBatterySaves => !DisableBatterySaves;

        public bool Debug { get; set; }

        public bool Headless { get; set; }

        public static void PrintUsage(TextWriter stream)
        {
            stream.WriteLine("Usage:");
            stream.WriteLine("DotNetGB.exe [OPTIONS] ROM_FILE");
            stream.WriteLine();
            stream.WriteLine("Available options:");
            stream.WriteLine("  -d  --force-dmg                Emulate classic GB (DMG) for universal ROMs");
            stream.WriteLine("  -c  --force-cgb                Emulate color GB (CGB) for all ROMs");
            stream.WriteLine("  -b  --use-bootstrap            Start with the GB bootstrap");
            stream.WriteLine("  -db --disable-battery-saves    Disable battery saves");
            stream.WriteLine("      --debug                    Enable debug console");
            stream.WriteLine("      --headless                 Start in the headless mode");
        }
    }
}