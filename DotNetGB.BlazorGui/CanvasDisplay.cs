using System.Threading;
using DotNetGB.Hardware;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DotNetGB.BlazorGui
{
    public class CanvasDisplay : IDisplay
    {
        private readonly Dispatcher _dispatcher;
        
        private readonly IJSRuntime _jsRuntime;

        public const int DISPLAY_WIDTH = 160;

        public const int DISPLAY_HEIGHT = 144;

        private const int STRIDE = DISPLAY_WIDTH * 4; // 4 bytes (32 bits) per pixel - RGBA

        public static readonly int[] COLORS = {
            0xe6f8da, 0x99c886, 0x437969, 0x051f2a
        };

        private readonly byte[] _pixels;

        private volatile bool _enabled;

        private volatile bool _doStop;

        private volatile bool _doRefresh;
        
        private int _i;
        
        public CanvasDisplay(Dispatcher dispatcher, IJSRuntime jsRuntime)
        {
            _dispatcher = dispatcher;
            _jsRuntime = jsRuntime;
            _pixels = new byte[DISPLAY_WIDTH * DISPLAY_HEIGHT * STRIDE];
        }

        public void PutDmgPixel(int color)
        {
            int c = COLORS[color];
            byte r = (byte)(c >> 16);
            byte g = (byte)((c >> 8) & 0xff);
            byte b = (byte)(c & 0xff);

            _pixels[_i++] = r;
            _pixels[_i++] = g;
            _pixels[_i++] = b;
            _pixels[_i++] = 255; // alpha
        }

        public void PutColorPixel(int gbcRgb)
        {
            byte r = (byte)((gbcRgb & 0x1f) * 8);
            byte g = (byte)(((gbcRgb >> 5) & 0x1f) * 8);
            byte b = (byte)(((gbcRgb >> 10) & 0x1f) * 8);

            _pixels[_i++] = r;
            _pixels[_i++] = g;
            _pixels[_i++] = b;
            _pixels[_i++] = 255; // alpha
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
                    _dispatcher.InvokeAsync(DrawImage);

                    lock (this)
                    {
                        _i = 0;
                        _doRefresh = false;
                        Monitor.PulseAll(this);
                    }
                }
            }
        }

        public void Stop()
        {
            _doStop = true;
        }

        private void DrawImage()
        {
            _jsRuntime.InvokeVoidAsync("drawCanvasPixels", _pixels);
        }
    }
}
