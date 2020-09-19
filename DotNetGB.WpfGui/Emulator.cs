using System;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using DotNetGB.Hardware;

namespace DotNetGB.WpfGui
{
    public class Emulator
    {
        private readonly IDisplay _display;

        private readonly Gameboy _gameboy;

        private readonly Thread _displayThread;

        private readonly Thread _gameboyThread;

        public Emulator(GameboyOptions options, IDisplay display, IController controller, ISoundOutput soundOutput)
        {
            Rom = new Cartridge(options);

            _display = display;
            _gameboy = new Gameboy(options, Rom, _display, controller, soundOutput, new NullSerialEndpoint());

            _displayThread = new Thread(_display.Run);
            _gameboyThread = new Thread(_gameboy.Run);
        }

        public Cartridge Rom { get; }

        public static GameboyOptions? ParseArgs(string[] args)
        {
            try
            {
                return CreateGameboyOptions(args);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine();
                return null;
            }
        }

        private static GameboyOptions CreateGameboyOptions(string[] args)
        {
            string? romPath = null;

            foreach (string a in args)
            {
                romPath = a;
            }

            if (romPath == null)
            {
                throw new ArgumentException("ROM path hasn't been specified");
            }

            var romFile = new FileInfo(romPath);

            return new GameboyOptions(romFile);
        }

        public void Run()
        {
            Dispatcher.CurrentDispatcher.InvokeAsync(StartGui);
        }

        private void StartGui()
        {
            _displayThread.Start();
            _gameboyThread.Start();
        }

        public void Stop()
        {
            _gameboy.Stop();
            _display.Stop();
        }
    }
}
