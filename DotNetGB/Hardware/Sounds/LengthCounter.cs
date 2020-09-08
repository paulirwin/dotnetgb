using static DotNetGB.Gameboy;

namespace DotNetGB.Hardware.Sounds
{
    public class LengthCounter
    {
        private readonly int DIVIDER = TICKS_PER_SEC / 256;
        private readonly int fullLength;
        private int length;
        private long i;
        private bool enabled;

        public LengthCounter(int fullLength)
        {
            this.fullLength = fullLength;
        }

        public void Start()
        {
            i = 8192;
        }

        public void Tick()
        {
            if (++i == DIVIDER)
            {
                i = 0;
                if (enabled && length > 0)
                {
                    length--;
                }
            }
        }

        public void SetLength(int length)
        {
            if (length == 0)
            {
                this.length = fullLength;
            }
            else
            {
                this.length = length;
            }
        }

        public void SetNr4(int value)
        {
            bool enable = (value & (1 << 6)) != 0;
            bool trigger = (value & (1 << 7)) != 0;
            if (enabled)
            {
                if (length == 0 && trigger)
                {
                    if (enable && i < DIVIDER / 2)
                    {
                        SetLength(fullLength - 1);
                    }
                    else
                    {
                        SetLength(fullLength);
                    }
                }
            }
            else if (enable)
            {
                if (length > 0 && i < DIVIDER / 2)
                {
                    length--;
                }

                if (length == 0 && trigger && i < DIVIDER / 2)
                {
                    SetLength(fullLength - 1);
                }
            }
            else
            {
                if (length == 0 && trigger)
                {
                    SetLength(fullLength);
                }
            }

            this.enabled = enable;
        }

        public int Value => length;

        public bool IsEnabled => enabled;

        public override string ToString()
        {
            return $"LengthCounter[l={length},f={fullLength},c={i},{(enabled ? "enabled" : "disabled")}]";
        }

        public void Reset()
        {
            this.enabled = true;
            this.i = 0;
            this.length = 0;
        }
    }
}