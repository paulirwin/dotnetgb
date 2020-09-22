using System.IO;
using DotNetGB.Hardware;

namespace DotNetGB.BlazorGui
{
    public class Emulator
    {
        public Emulator(Stream romStream, IDisplay display)
        {
            var options = new GameboyOptions();
            Rom = new Cartridge(options, romStream);
        }

        public Cartridge Rom { get; set; }
    }
}
