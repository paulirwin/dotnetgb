using System;

namespace DotNetGB.Hardware.Sounds
{
    public class Sound : IAddressSpace, ITickable
    {
        private static readonly int[] MASKS = {
            0x80, 0x3f, 0x00, 0xff, 0xbf,
            0xff, 0x3f, 0x00, 0xff, 0xbf,
            0x7f, 0xff, 0x9f, 0xff, 0xbf,
            0xff, 0xff, 0x00, 0x00, 0xbf,
            0x00, 0x00, 0x70,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        private readonly AbstractSoundMode[] allModes;

        private readonly AbstractSoundMode mode1;
        private readonly AbstractSoundMode mode2;
        private readonly AbstractSoundMode mode3; 
        private readonly AbstractSoundMode mode4;

        private readonly Ram r = new Ram(0xff24, 0x03);

        private readonly ISoundOutput output;

        private readonly int[] channels = new int[4];

        private bool enabled;

        //private readonly bool[] overridenEnabled = { true, true, true, true };

        public Sound(ISoundOutput output, bool gbc)
        {
            mode1 = new SoundMode1(gbc);
            mode2 = new SoundMode2(gbc);
            mode3 = new SoundMode3(gbc);
            mode4 = new SoundMode4(gbc);
            allModes = new[] {mode1, mode2, mode3, mode4};
            this.output = output;
        }

        public void Tick()
        {
            if (!enabled)
            {
                return;
            }

            channels[0] = mode1.Tick();
            channels[1] = mode2.Tick();
            channels[2] = mode3.Tick();
            channels[3] = mode4.Tick();

            int selection = r[0xff25];
            int left = 0;
            int right = 0;
            for (int i = 0; i < 4; i++)
            {
                //if (!overridenEnabled[i])
                //{
                //    continue;
                //}
                if ((selection & (1 << i + 4)) != 0)
                {
                    left += channels[i];
                }
                if ((selection & (1 << i)) != 0)
                {
                    right += channels[i];
                }
            }
            left /= 4;
            right /= 4;

            int volumes = r[0xff24];
            left *= ((volumes >> 4) & 0b111);
            right *= (volumes & 0b111);

            output.Play((byte)left, (byte)right);
        }

        private IAddressSpace? GetAddressSpace(int address)
        {
            if (mode1.Accepts(address))
                return mode1;
            if (mode2.Accepts(address))
                return mode2;
            if (mode3.Accepts(address))
                return mode3;
            if (mode4.Accepts(address))
                return mode4;
            if (r.Accepts(address))
                return r;
            return null;
        }

        public bool Accepts(int address)
        {
            return GetAddressSpace(address) != null;
        }

        public int this[int address]
        {
            get
            {
                int result;
                if (address == 0xff26)
                {
                    result = 0;
                    for (int i = 0; i < allModes.Length; i++)
                    {
                        result |= allModes[i].IsEnabled ? (1 << i) : 0;
                    }
                    result |= enabled ? (1 << 7) : 0;
                }
                else
                {
                    result = GetUnmaskedByte(address);
                }
                return result | MASKS[address - 0xff10];
            }
            set
            {
                if (address == 0xff26)
                {
                    if ((value & (1 << 7)) == 0)
                    {
                        if (enabled)
                        {
                            enabled = false;
                            Stop();
                        }
                    }
                    else
                    {
                        if (!enabled)
                        {
                            enabled = true;
                            Start();
                        }
                    }
                    return;
                }

                var s = GetAddressSpace(address);
                System.Diagnostics.Debug.Assert(s != null, "Unknown address space");
                s[address] = value;
            }
        }
        
        private int GetUnmaskedByte(int address)
        {
            var s = GetAddressSpace(address);
            System.Diagnostics.Debug.Assert(s != null, "Unknown address space");
            return s[address];
        }

        private void Start()
        {
            for (int i = 0xff10; i <= 0xff25; i++)
            {
                int v = 0;
                // lengths should be preserved
                if (i == 0xff11 || i == 0xff16 || i == 0xff20)
                { // channel 1, 2, 4 lengths
                    v = GetUnmaskedByte(i) & 0b00111111;
                }
                else if (i == 0xff1b)
                { // channel 3 length
                    v = GetUnmaskedByte(i);
                }
                this[i] = v;
            }
            foreach (AbstractSoundMode m in allModes)
            {
                m.Start();
            }
            output.Start();
        }

        private void Stop()
        {
            output.Stop();
            foreach (AbstractSoundMode s in allModes)
            {
                s.Stop();
            }
        }

        //public void EnableChannel(int i, bool enabled)
        //{
        //    overridenEnabled[i] = enabled;
        //}
    }
}
