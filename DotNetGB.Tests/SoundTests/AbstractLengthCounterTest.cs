using System;
using DotNetGB.Hardware.Sounds;

namespace DotNetGB.Tests.SoundTests
{
    public abstract class AbstractLengthCounterTest
    {
        protected readonly int maxlen;

        protected readonly LengthCounter lengthCounter;

        protected AbstractLengthCounterTest()
            : this(256)
        {
        }

        protected AbstractLengthCounterTest(int maxlen)
        {
            this.maxlen = maxlen;
            this.lengthCounter = new LengthCounter(maxlen);
        }

        protected void Wchn(int register, int value)
        {
            if (register == 1)
            {
                lengthCounter.SetLength(0 - value);
            }
            else if (register == 4)
            {
                lengthCounter.SetNr4(value);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        protected void DelayClocks(int clocks)
        {
            for (int i = 0; i < clocks; i++)
            {
                lengthCounter.Tick();
            }
        }

        protected void DelayApu(int apuUnit)
        {
            DelayClocks(apuUnit * (Gameboy.TICKS_PER_SEC / 256));
        }

        protected void SyncApu()
        {
            lengthCounter.Reset();
        }
    }
}
