namespace DotNetGB.Hardware.GpuPhases
{
    public class VBlankPhase : IGpuPhase
    {
        private int _ticks;

        public VBlankPhase Start()
        {
            _ticks = 0;
            return this;
        }

        public bool Tick() => ++_ticks < 456;
    }
}
