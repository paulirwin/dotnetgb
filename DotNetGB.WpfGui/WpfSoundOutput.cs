using System;
using DotNetGB.Hardware;
using NAudio.Utils;
using NAudio.Wave;

namespace DotNetGB.WpfGui
{
    public class WpfSoundOutput : ISoundOutput, IWaveProvider
    {
        private const int SAMPLE_RATE = 22050;

        private const int BUFFER_SIZE = 128;
        
        private const int DIVIDER = Gameboy.TICKS_PER_SEC / SAMPLE_RATE;
        
        private readonly WaveOut _player = new WaveOut();

        private readonly byte[] _buffer = new byte[BUFFER_SIZE];

        private readonly CircularBuffer _circularBuffer = new CircularBuffer(8192);
        
        private int _i;

        private int _tick;
        
        public bool Enabled { get; set; }

        private int _leftAvg;

        private int _rightAvg;

        public void Start()
        {
            if (_player.PlaybackState == PlaybackState.Playing)
                return;

            _circularBuffer.Write(_buffer, 0, BUFFER_SIZE);
            
            _player.Init(this);
            _player.Play();
        }

        public void Stop()
        {
            if (_player.PlaybackState == PlaybackState.Stopped)
                return;

            _player.Stop();
        }

        public void Play(byte left, byte right)
        {
            if (_tick++ != 0)
            {
                _leftAvg += left;
                _rightAvg += right;
                _tick %= DIVIDER;
                return;
            }

            _buffer[_i++] = (byte) (_leftAvg / DIVIDER);
            _buffer[_i++] = (byte) (_rightAvg / DIVIDER);

            _leftAvg = _rightAvg = 0;

            if (_i == BUFFER_SIZE)
            {
                _circularBuffer.Write(_buffer, 0, BUFFER_SIZE);
                _i = 0;
            }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            int read = _circularBuffer.Read(buffer, offset, count);

            if (read < count)
            {
                Array.Clear(buffer, offset + read, count - read);
                read = count;
            }

            return read;
        }

        public WaveFormat WaveFormat { get; } = new WaveFormat(SAMPLE_RATE, 8, 2);
    }
}