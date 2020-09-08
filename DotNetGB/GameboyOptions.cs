using System.IO;

namespace DotNetGB
{
    public class GameboyOptions
    {
        public GameboyOptions(FileInfo romFile)
        {
            RomFile = romFile;
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
            throw new System.NotImplementedException();
        }
    }
}