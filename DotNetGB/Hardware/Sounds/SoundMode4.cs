namespace DotNetGB.Hardware.Sounds
{
    public class SoundMode4 : AbstractSoundMode
    {
        private readonly VolumeEnvelope volumeEnvelope;

        private readonly PolynomialCounter polynomialCounter;

        private int lastResult;

        private readonly Lfsr lfsr = new Lfsr();

        public SoundMode4(bool gbc)
            : base(0xff1f, 64, gbc)
        {
            volumeEnvelope = new VolumeEnvelope();
            this.polynomialCounter = new PolynomialCounter();
        }

        public override void Start()
        {
            if (gbc)
            {
                length.Reset();
            }
            length.Start();
            lfsr.Start();
            volumeEnvelope.Start();
        }

        protected override void Trigger()
        {
            lfsr.Reset();
            volumeEnvelope.Trigger();
        }

        public override int Tick()
        {
            volumeEnvelope.Tick();

            if (!UpdateLength())
            {
                return 0;
            }
            if (!dacEnabled)
            {
                return 0;
            }

            if (polynomialCounter.Tick())
            {
                lastResult = lfsr.NextBit((nr3 & (1 << 3)) != 0);
            }
            return lastResult * volumeEnvelope.Volume;
        }

        protected override int Nr1
        {
            get => base.Nr1;
            set
            {
                base.Nr1 = value;
                length.SetLength(64 - (value & 0b00111111));
            }
        }

        protected override int Nr2
        {
            get => base.Nr2;
            set
            {
                base.Nr2 = value;
                volumeEnvelope.Nr2 = value;
                dacEnabled = (value & 0b11111000) != 0;
                channelEnabled &= dacEnabled;
            }
        }

        protected override int Nr3
        {
            get => base.Nr3;
            set
            {
                base.Nr3 = value;
                polynomialCounter.Nr43 = value;
            }
        }
    }
}
