﻿using DotNetGB.Hardware;
using NAudio.Wave;

namespace DotNetGB.WpfGui
{
    public class WpfSoundOutput : ISoundOutput
    {
        private const int SAMPLE_RATE = 22050;

        private const int BUFFER_SIZE = 1024;

        private const int DIVIDER = Gameboy.TICKS_PER_SEC / SAMPLE_RATE;

        private readonly WaveFormat _format = new WaveFormat(SAMPLE_RATE, 8, 2);

        private readonly BufferedWaveProvider _provider;

        private readonly IWavePlayer _player = new WaveOut();

        private readonly byte[] _buffer = new byte[BUFFER_SIZE];

        private int _i;

        private int _tick;

        public WpfSoundOutput()
        {
            _provider = new BufferedWaveProvider(_format);
        }

        public void Start()
        {
            _player.Init(_provider);
            _player.Play();
        }

        public void Stop()
        {
            _provider.ClearBuffer();
            _player.Stop();
        }

        public void Play(int left, int right)
        {
            if (_tick++ != 0)
            {
                _tick %= DIVIDER;
                return;
            }

            _buffer[_i++] = (byte) left;
            _buffer[_i++] = (byte) right;

            if (_i >= BUFFER_SIZE)
            {
                _provider.AddSamples(_buffer, 0, _i);
                _i = 0;
            }
        }
    }
}