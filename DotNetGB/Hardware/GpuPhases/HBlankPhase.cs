﻿namespace DotNetGB.Hardware.GpuPhases
{
    public class HBlankPhase : IGpuPhase
    {
        private int _ticks;

        public HBlankPhase Start(int ticksInLine)
        {
            _ticks = ticksInLine;
            return this;
        }

        public bool Tick()
        {
            _ticks++;
            return _ticks < 456;
        }
    }
}
