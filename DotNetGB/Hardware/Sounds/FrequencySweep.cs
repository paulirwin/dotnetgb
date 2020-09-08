namespace DotNetGB.Hardware.Sounds
{
    public class FrequencySweep
    {
        private const int DIVIDER = Gameboy.TICKS_PER_SEC / 128;

        // sweep parameters
        private int period;

        private bool negate;

        private int shift;

        // current process variables
        private int timer;

        private int shadowFreq;

        private int nr13, nr14;

        private int i;

        private bool overflow;

        private bool counterEnabled;

        private bool negging;

        public void Start()
        {
            counterEnabled = false;
            i = 8192;
        }

        public void Trigger()
        {
            this.negging = false;
            this.overflow = false;

            this.shadowFreq = nr13 | ((nr14 & 0b111) << 8);
            this.timer = period == 0 ? 8 : period;
            this.counterEnabled = period != 0 || shift != 0;

            if (shift > 0)
            {
                Calculate();
            }
        }

        public int Nr10
        {
            set
            {
                this.period = (value >> 4) & 0b111;
                this.negate = (value & (1 << 3)) != 0;
                this.shift = value & 0b111;
                if (negging && !negate)
                {
                    overflow = true;
                }
            }
        }

        public int Nr13
        {
            get => nr13;
            set => nr13 = value;
        }

        public int Nr14
        {
            get => nr14;
            set
            {
                this.nr14 = value;
                if ((value & (1 << 7)) != 0)
                {
                    Trigger();
                }
            }
        }

        public void Tick()
        {
            if (++i == DIVIDER)
            {
                i = 0;
                if (!counterEnabled)
                {
                    return;
                }
                if (--timer == 0)
                {
                    timer = period == 0 ? 8 : period;
                    if (period != 0)
                    {
                        int newFreq = Calculate();
                        if (!overflow && shift != 0)
                        {
                            shadowFreq = newFreq;
                            nr13 = shadowFreq & 0xff;
                            nr14 = (shadowFreq & 0x700) >> 8;
                            Calculate();
                        }
                    }
                }
            }
        }

        private int Calculate()
        {
            int freq = shadowFreq >> shift;
            if (negate)
            {
                freq = shadowFreq - freq;
                negging = true;
            }
            else
            {
                freq = shadowFreq + freq;
            }
            if (freq > 2047)
            {
                overflow = true;
            }
            return freq;
        }

        public bool IsEnabled => !overflow;
    }
}