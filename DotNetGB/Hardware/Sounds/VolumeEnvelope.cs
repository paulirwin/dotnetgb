namespace DotNetGB.Hardware.Sounds
{
    public class VolumeEnvelope
    {
        private int initialVolume;

        private int envelopeDirection;

        private int sweep;

        private int volume;

        private int i;

        private bool finished;

        public int Nr2
        {
            set
            {
                this.initialVolume = value >> 4;
                this.envelopeDirection = (value & (1 << 3)) == 0 ? -1 : 1;
                this.sweep = value & 0b111;
            }
        }

        public bool IsEnabled => sweep > 0;

        public void Start()
        {
            finished = true;
            i = 8192;
        }

        public void Trigger()
        {
            volume = initialVolume;
            i = 0;
            finished = false;
        }

        public void Tick()
        {
            if (finished)
            {
                return;
            }
            if ((volume == 0 && envelopeDirection == -1) || (volume == 15 && envelopeDirection == 1))
            {
                finished = true;
                return;
            }
            if (++i == sweep * Gameboy.TICKS_PER_SEC / 64)
            {
                i = 0;
                volume += envelopeDirection;
            }
        }

        public int Volume
        {
            get
            {
                if (IsEnabled)
                {
                    return volume;
                }
                else
                {
                    return initialVolume;
                }
            }
        }
    }
}