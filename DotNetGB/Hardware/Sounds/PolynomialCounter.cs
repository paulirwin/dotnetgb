using System;

namespace DotNetGB.Hardware.Sounds
{
    public class PolynomialCounter
    {
        private int shiftedDivisor;

        private int i;

        public int Nr43
        {
            set
            {
                int clockShift = value >> 4;
                int divisor = (value & 0b111) switch
                {
                    0 => 8,
                    1 => 16,
                    2 => 32,
                    3 => 48,
                    4 => 64,
                    5 => 80,
                    6 => 96,
                    7 => 112,
                    _ => throw new InvalidOperationException()
                };

                shiftedDivisor = divisor << clockShift;
                i = 1;
            }
        }

        public bool Tick()
        {
            if (--i == 0)
            {
                i = shiftedDivisor;
                return true;
            }

            return false;
        }
    }
}
