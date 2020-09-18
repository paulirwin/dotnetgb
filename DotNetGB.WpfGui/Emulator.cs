using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using DotNetGB.Hardware;

namespace DotNetGB.WpfGui
{
    public class Emulator
    {
        private const int SCALE = 2;

        private readonly GameboyOptions _options;

        private readonly Cartridge _rom;

        private readonly WpfSoundOutput _sound;

        private readonly MainWindow _display;

        private readonly WpfController _controller;

        private readonly Gameboy _gameboy;
        
        public Emulator(string[] args)
        {
            _options = ParseArgs(args);
            _rom = new Cartridge(_options);

            _sound = new WpfSoundOutput();
            _display = new MainWindow();
            _controller = new WpfController();
            _gameboy = new Gameboy(_options, _rom, _display, _controller, _sound, new NullSerialEndpoint());
        }

        private static GameboyOptions ParseArgs(string[] args)
        {
            if (args.Length == 0)
            {
                GameboyOptions.PrintUsage(Console.Out);
                Environment.Exit(0);
                return null;
            }

            try
            {
                return CreateGameboyOptions(args);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine();
                GameboyOptions.PrintUsage(Console.Error);
                Environment.Exit(1);
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
            _display.SetPreferredSize(new Size(160 * SCALE, 144 * SCALE));

            _display.Title = $"DotNet GB: {_rom.Title}";
            _display.Show();

            _display.AddKeyListener(_controller);

            new Thread(_display.Run).Start();
            new Thread(_gameboy.Run).Start();
        }
    }
}
