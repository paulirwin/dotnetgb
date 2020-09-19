using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace DotNetGB.WpfGui
{
    public partial class MainWindow
    {
        private Emulator? _emulator;

        private readonly WpfController _controller = new WpfController();

        private readonly WpfSoundOutput _soundOutput = new WpfSoundOutput();

        private GameboyOptions? _options;

        private bool _isFullScreen;

        public MainWindow(string[] args)
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Closed += (s, e) => Environment.Exit(0);
            
            if (args.Length > 0)
            {
                _options = Emulator.ParseArgs(args);
                LoadEmulator();
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

        private bool IsFullScreen
        {
            get => _isFullScreen;
            set
            {
                if (value)
                {
                    EnterFullScreen();
                }
                else
                {
                    ExitFullScreen();
                }

                _isFullScreen = value;
            }
        }

        private void EnterFullScreen()
        {
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
            MainMenu.Visibility = Visibility.Collapsed;
        }

        private void ExitFullScreen()
        {
            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.SingleBorderWindow;
            MainMenu.Visibility = Visibility.Visible;
        }

        private void AddKeyListener()
        {
            KeyDown += (sender, args) =>
            {
                _controller.ButtonPressed(args.Key);
            };

            KeyUp += (sender, args) =>
            {
                if (args.Key == Key.Escape && IsFullScreen)
                {
                    IsFullScreen = false;
                    return;
                }

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
            LoadEmulator();
        }

        private void LoadEmulator()
        {
            if (_options == null)
                return;

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

        private void MenuItemFullscreen_OnClick(object sender, RoutedEventArgs e)
        {
            IsFullScreen = true;
        }
    }
}
