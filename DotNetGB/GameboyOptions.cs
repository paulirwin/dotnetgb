using System;
using System.Collections.Generic;
using System.IO;

namespace DotNetGB
{
    public class GameboyOptions
    {
        public GameboyOptions(string romFile)
            : this(new FileInfo(romFile))
        {
        }

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

        public GameboyOptions WithRomFile(string romFile)
        {
            return new GameboyOptions(romFile)
            {
                Debug = Debug,
                DisableBatterySaves = DisableBatterySaves,
                ForceCgb = ForceCgb,
                ForceDmg = ForceDmg,
                Headless = Headless,
                UseBootstrap = UseBootstrap,
            };
        }
    }
}