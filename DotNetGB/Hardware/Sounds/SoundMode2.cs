using System;

namespace DotNetGB.Hardware.Sounds
{
    public class SoundMode2 : AbstractSoundMode
    {
        private int freqDivider;

        private int lastOutput;

        private int i;

        private VolumeEnvelope volumeEnvelope;

        public SoundMode2(bool gbc)
            : base(0xff15, 64, gbc)
        {
            this.volumeEnvelope = new VolumeEnvelope();
        }

        public override void Start()
        {
            i = 0;
            if (gbc)
            {
                length.Reset();
            }
            length.Start();
            volumeEnvelope.Start();
        }

        protected override void Trigger()
        {
            this.i = 0;
            freqDivider = 1;
            volumeEnvelope.Trigger();
        }
        public override int Tick()
        {
            volumeEnvelope.Tick();

            bool e = true;
            e = UpdateLength() && e;
            e = dacEnabled && e;
            if (!e)
            {
                return 0;
            }

            if (--freqDivider == 0)
            {
                ResetFreqDivider();
                lastOutput = ((Duty & (1 << i)) >> i);
                i = (i + 1) % 8;
            }
            return lastOutput * volumeEnvelope.Volume;
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

        private int Duty
        {
            get
            {
                switch (Nr1 >> 6)
                {
                    case 0:
                        return 0b00000001;
                    case 1:
                        return 0b10000001;
                    case 2:
                        return 0b10000111;
                    case 3:
                        return 0b01111110;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        private void ResetFreqDivider()
        {
            freqDivider = Frequency * 4;
        }
    }
}
