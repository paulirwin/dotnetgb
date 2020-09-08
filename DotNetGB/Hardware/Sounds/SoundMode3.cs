using System;

namespace DotNetGB.Hardware.Sounds
{
    public class SoundMode3 : AbstractSoundMode
    {
        private static readonly int[] DMG_WAVE = new int[]
        {
            0x84, 0x40, 0x43, 0xaa, 0x2d, 0x78, 0x92, 0x3c,
            0x60, 0x59, 0x59, 0xb0, 0x34, 0xb8, 0x2e, 0xda
        };

        private static readonly int[] CGB_WAVE = new int[]
        {
            0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff,
            0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff
        };

        private readonly Ram waveRam = new Ram(0xff30, 0x10);

        private int freqDivider;

        private int lastOutput;

        private int i;

        private int ticksSinceRead = 65536;

        private int lastReadAddr;

        private int buffer;

        private bool triggered;

        public SoundMode3(bool gbc)
            : base(0xff1a, 256, gbc)
        {
            foreach (int v in gbc ? CGB_WAVE : DMG_WAVE)
            {
                waveRam[0xff30] = v;
            }
        }

        public override bool Accepts(int address)
        {
            return waveRam.Accepts(address) || base.Accepts(address);
        }

        public override int this[int address]
        {
            get
            {
                if (!waveRam.Accepts(address))
                {
                    return base[address];
                }

                if (!IsEnabled)
                {
                    return waveRam[address];
                }
                else if (waveRam.Accepts(lastReadAddr) && (gbc || ticksSinceRead < 2))
                {
                    return waveRam[lastReadAddr];
                }
                else
                {
                    return 0xff;
                }
            }
            set
            {
                if (!waveRam.Accepts(address))
                {
                    base[address] = value;
                    return;
                }

                if (!IsEnabled)
                {
                    waveRam[address] = value;
                }
                else if (waveRam.Accepts(lastReadAddr) && (gbc || ticksSinceRead < 2))
                {
                    waveRam[lastReadAddr] = value;
                }
            }
        }

        protected override int Nr0
        {
            get => base.Nr0;
            set
            {
                base.Nr0 = value;
                dacEnabled = (value & (1 << 7)) != 0;
                channelEnabled &= dacEnabled;
            }
        }

        protected override int Nr1
        {
            get => base.Nr1;
            set
            {
                base.Nr1 = value;
                length.SetLength(256 - value);
            }
        }

        protected override int Nr4
        {
            get => base.Nr4;
            set
            {
                if (!gbc && (value & (1 << 7)) != 0)
                {
                    if (IsEnabled && freqDivider == 2)
                    {
                        int pos = i / 2;
                        if (pos < 4)
                        {
                            waveRam[0xff30] = waveRam[0xff30 + pos];
                        }
                        else
                        {
                            pos = pos & ~3;
                            for (int j = 0; j < 4; j++)
                            {
                                waveRam[0xff30 + j] = waveRam[0xff30 + ((pos + j) % 0x10)];
                            }
                        }
                    }
                }

                base.Nr4 = value;
            }
        }
        
        public override void Start()
        {
            i = 0;
            buffer = 0;
            if (gbc)
            {
                length.Reset();
            }

            length.Start();
        }

        protected override void Trigger()
        {
            i = 0;
            freqDivider = 6;
            triggered = !gbc;
            if (gbc)
            {
                GetWaveEntry();
            }
        }

        public override int Tick()
        {
            ticksSinceRead++;
            if (!UpdateLength())
            {
                return 0;
            }

            if (!dacEnabled)
            {
                return 0;
            }

            if ((Nr0 & (1 << 7)) == 0)
            {
                return 0;
            }

            if (--freqDivider == 0)
            {
                ResetFreqDivider();
                if (triggered)
                {
                    lastOutput = (buffer >> 4) & 0x0f;
                    triggered = false;
                }
                else
                {
                    lastOutput = GetWaveEntry();
                }

                i = (i + 1) % 32;
            }

            return lastOutput;
        }

        private int Volume => (Nr2 >> 5) & 0b11;

        private int GetWaveEntry()
        {
            ticksSinceRead = 0;
            lastReadAddr = 0xff30 + i / 2;
            buffer = waveRam[lastReadAddr];
            int b = buffer;
            if (i % 2 == 0)
            {
                b = (b >> 4) & 0x0f;
            }
            else
            {
                b = b & 0x0f;
            }

            switch (Volume)
            {
                case 0:
                    return 0;
                case 1:
                    return b;
                case 2:
                    return b >> 1;
                case 3:
                    return b >> 2;
                default:
                    throw new InvalidOperationException();
            }
        }

        private void ResetFreqDivider()
        {
            freqDivider = Frequency * 2;
        }
    }
}