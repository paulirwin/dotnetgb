using System;

namespace DotNetGB.Hardware.Sounds
{
    public abstract class AbstractSoundMode : IAddressSpace
    {
        protected readonly int offset;

        protected readonly bool gbc;

        protected bool channelEnabled;

        protected bool dacEnabled;

        protected int nr0, nr1, nr2, nr3, nr4;

        protected LengthCounter length;

        protected AbstractSoundMode(int offset, int length, bool gbc)
        {
            this.offset = offset;
            this.length = new LengthCounter(length);
            this.gbc = gbc;
        }

        public abstract int Tick();

        protected abstract void Trigger();

        public bool IsEnabled => channelEnabled && dacEnabled;

        public virtual bool Accepts(int address)
        {
            return address >= offset && address < offset + 5;
        }
        
        public virtual int this[int address]
        {
            get
            {
                switch (address - offset)
                {
                    case 0:
                        return Nr0;

                    case 1:
                        return Nr1;

                    case 2:
                        return Nr2;

                    case 3:
                        return Nr3;

                    case 4:
                        return Nr4;

                    default:
                        throw new ArgumentException($"Illegal address for sound mode: {address:x2}");
                }
            }
            set
            {
                switch (address - offset)
                {
                    case 0:
                        Nr0 = value;
                        break;

                    case 1:
                        Nr1 = value;
                        break;

                    case 2:
                        Nr2 = value;
                        break;

                    case 3:
                        Nr3 = value;
                        break;

                    case 4:
                        Nr4 = value;
                        break;
                }
            }
        }

        protected virtual int Nr0
        {
            get => nr0;
            set => nr0 = value;
        }

        protected virtual int Nr1
        {
            get => nr1;
            set => nr1 = value;
        }

        protected virtual int Nr2
        {
            get => nr2;
            set => nr2 = value;
        }

        protected virtual int Nr3
        {
            get => nr3;
            set => nr3 = value;
        }

        protected virtual int Nr4
        {
            get => nr4;
            set
            {
                nr4 = value;
                length.SetNr4(value);
                if ((value & (1 << 7)) != 0)
                {
                    channelEnabled = dacEnabled;
                    Trigger();
                }
            }
        }
        
        protected int Frequency => 2048 - (Nr3 | ((Nr4 & 0b111) << 8));

        public abstract void Start();

        public void Stop()
        {
            channelEnabled = false;
        }

        protected bool UpdateLength()
        {
            length.Tick();
            if (!length.IsEnabled)
            {
                return channelEnabled;
            }
            if (channelEnabled && length.Value == 0)
            {
                channelEnabled = false;
            }
            return channelEnabled;
        }
    }
}
