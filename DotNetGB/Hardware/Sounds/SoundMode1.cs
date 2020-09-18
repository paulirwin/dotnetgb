using System;

namespace DotNetGB.Hardware.Sounds
{
    public class SoundMode1 : AbstractSoundMode
    {
        private int freqDivider;

        private int lastOutput;

        private int i;

        private readonly FrequencySweep frequencySweep;

        private readonly VolumeEnvelope volumeEnvelope;

        public SoundMode1(bool gbc)
            : base(0xff10, 64, gbc)
        {
            frequencySweep = new FrequencySweep();
            volumeEnvelope = new VolumeEnvelope();
        }

        public override void Start()
        {
            i = 0;
            if (gbc)
            {
                length.Reset();
            }

            length.Start();
            frequencySweep.Start();
            volumeEnvelope.Start();
        }

        protected override void Trigger()
        {
            i = 0;
            freqDivider = 1;
            volumeEnvelope.Trigger();
        }

        public override int Tick()
        {
            volumeEnvelope.Tick();

            bool e = true;
            e = UpdateLength() && e;
            e = UpdateSweep() && e;
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

        protected override int Nr0
        {
            get => base.Nr0;
            set
            {
                base.Nr0 = value;
                frequencySweep.Nr10 = value;
            }
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
            get => frequencySweep.Nr13;
            set
            {
                base.Nr3 = value;
                frequencySweep.Nr13 = value;
            }
        }

        protected override int Nr4
        {
            get => (base.Nr4 & 0b11111000) | (frequencySweep.Nr14 & 0b00000111);
            set
            {
                base.Nr4 = value;
                frequencySweep.Nr14 = value;
            }
        }

        private int Duty
        {
            get
            {
                return (Nr1 >> 6) switch
                {
                    0 => 0b00000001,
                    1 => 0b10000001,
                    2 => 0b10000111,
                    3 => 0b01111110,
                    _ => throw new InvalidOperationException()
                };
            }
        }

        private void ResetFreqDivider()
        {
            freqDivider = Frequency * 4;
        }

        protected bool UpdateSweep()
        {
            frequencySweep.Tick();
            if (channelEnabled && !frequencySweep.IsEnabled)
            {
                channelEnabled = false;
            }

            return channelEnabled;
        }
    }
}