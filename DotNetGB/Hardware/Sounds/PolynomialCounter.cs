﻿using System;

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
                int divisor;
                switch (value & 0b111)
                {
                    case 0:
                        divisor = 8;
                        break;

                    case 1:
                        divisor = 16;
                        break;

                    case 2:
                        divisor = 32;
                        break;

                    case 3:
                        divisor = 48;
                        break;

                    case 4:
                        divisor = 64;
                        break;

                    case 5:
                        divisor = 80;
                        break;

                    case 6:
                        divisor = 96;
                        break;

                    case 7:
                        divisor = 112;
                        break;

                    default:
                        throw new InvalidOperationException();
                }

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
            else
            {
                return false;
            }
        }
    }
}
