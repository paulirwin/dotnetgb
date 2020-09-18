using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DotNetGB.Hardware;
using Microsoft.Win32;

namespace DotNetGB.WpfGui
{
    public partial class MainWindow : IDisplay
    {
        public event EventHandler<string> OpenFile;

        public const int DISPLAY_WIDTH = 160;

        public const int DISPLAY_HEIGHT = 144;

        private const int STRIDE = DISPLAY_WIDTH * 3; // 3 bytes (24 bits) per pixel
        
        public static readonly int[] COLORS = {
            0xe6f8da, 0x99c886, 0x437969, 0x051f2a
        };

        private readonly byte[] _pixels;

        private volatile bool _enabled;

        private volatile bool _doStop;

        private volatile bool _doRefresh;

        private int _i;

        private readonly Int32Rect _rect = new Int32Rect(0, 0, DISPLAY_WIDTH, DISPLAY_HEIGHT);

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Closed += (sender, args) => Environment.Exit(0);

            _pixels = new byte[DISPLAY_WIDTH * DISPLAY_HEIGHT * STRIDE];
            Array.Fill(_pixels, (byte)0);
        }

        public WriteableBitmap Display { get; } = new WriteableBitmap(DISPLAY_WIDTH, DISPLAY_HEIGHT, 96d, 96d, PixelFormats.Rgb24, null);

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            DisplayImage.Source = Display;
            DrawImage();
        }

        public void SetPreferredSize(Size size)
        {
            Width = size.Width;
            Height = size.Height;
        }

        public void PutDmgPixel(int color)
        {
            int c = COLORS[color];
            byte r = (byte) (c >> 16);
            byte g = (byte) ((c >> 8) & 0xff);
            byte b = (byte) (c & 0xff);

            _pixels[_i++] = r;
            _pixels[_i++] = g;
            _pixels[_i++] = b;
        }

        public void PutColorPixel(int gbcRgb)
        {
            byte r = (byte) ((gbcRgb >> 0) & 0x1f);
            byte g = (byte) ((gbcRgb >> 5) & 0x1f);
            byte b = (byte) ((gbcRgb >> 10) & 0x1f);

            _pixels[_i++] = r;
            _pixels[_i++] = g;
            _pixels[_i++] = b;
        }
        
        public void RequestRefresh()
        {
            _doRefresh = true;
            lock (this)
            {
                Monitor.PulseAll(this);
            }
        }

        public void WaitForRefresh()
        {
            while (_doRefresh)
            {
                lock (this)
                {
                    try
                    {
                        Monitor.Wait(this, 1);
                    }
                    catch (ThreadInterruptedException)
                    {
                        break;
                    }
                }
            }
        }

        public void EnableLcd()
        {
            _enabled = true;
        }

        public void DisableLcd()
        {
            _enabled = false;
        }

        public void AddKeyListener(WpfController controller)
        {
            KeyDown += (sender, args) =>
            {
                controller.ButtonPressed(args.Key);
            };

            KeyUp += (sender, args) =>
            {
                controller.ButtonReleased(args.Key);
            };
        }

        public void Run()
        {
            _doStop = false;
            _doRefresh = false;
            _enabled = true;

            while (!_doStop)
            {
                lock (this)
                {
                    try
                    {
                        Monitor.Wait(this, 1);
                    }
                    catch (ThreadInterruptedException)
                    {
                        break;
                    }
                }

                if (_doRefresh)
                {
                    Dispatcher.InvokeAsync(DrawImage);

                    lock (this)
                    {
                        _i = 0;
                        _doRefresh = false;
                        Monitor.PulseAll(this);
                    }
                }
            }
        }

        private void DrawImage()
        {
            Display.Lock();

            Display.WritePixels(_rect, _pixels, STRIDE, 0);

            Display.Unlock();
        }

        private void MenuItemExit_OnClick(object sender, RoutedEventArgs e) => Environment.Exit(0);

        private void MenuItemOpen_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "ROM Files (*.gb,*.gbc;*.rom)|*.gb;*.gbc;*.rom|All Files (*.*)|*.*",
            };

            if (dialog.ShowDialog().GetValueOrDefault())
            {
                OnOpenFile(dialog.FileName);
            }
        }

        protected virtual void OnOpenFile(string fileName)
        {
            OpenFile?.Invoke(this, fileName);
        }
    }
}
