using System;
using System.Windows;
using Microsoft.Win32;

namespace DotNetGB.WpfGui
{
    public partial class MainWindow
    {
        private Emulator? _emulator;

        private readonly WpfController _controller = new WpfController();

        private readonly WpfSoundOutput _soundOutput = new WpfSoundOutput();

        private GameboyOptions? _options;

        public MainWindow(string[] args)
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Closed += (s, e) => Environment.Exit(0);

            if (args.Length > 0)
            {
                _options = Emulator.ParseArgs(args);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetPreferredSize(new Size(320, 288));
            AddKeyListener();
        }

        private void SetPreferredSize(Size size)
        {
            Width = size.Width;
            Height = size.Height;
        }
        
        private void AddKeyListener()
        {
            KeyDown += (sender, args) =>
            {
                _controller.ButtonPressed(args.Key);
            };

            KeyUp += (sender, args) =>
            {
                _controller.ButtonReleased(args.Key);
            };
        }

        private void MenuItemExit_OnClick(object sender, RoutedEventArgs e) => Environment.Exit(0);

        private void MenuItemOpen_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "ROM Files (*.gb,*.gbc;*.rom)|*.gb;*.gbc;*.rom|All Files (*.*)|*.*",
            };

            if (!dialog.ShowDialog().GetValueOrDefault()) 
                return;

            _emulator?.Stop();
            
            _options = _options == null ? new GameboyOptions(dialog.FileName) : _options.WithRomFile(dialog.FileName);
            _emulator = new Emulator(_options, EmulatorDisplay, _controller, _soundOutput);

            Title = $"DotNetGB: {_emulator.Rom.Title}";
                
            _emulator.Run();
        }

        private void MenuItemAudioEnabled_OnChecked(object sender, RoutedEventArgs e)
        {
            _soundOutput.Enabled = true;
            _soundOutput.Start();
        }

        private void MenuItemAudioEnabled_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _soundOutput.Enabled = false;
            _soundOutput.Stop();
        }
    }
}
