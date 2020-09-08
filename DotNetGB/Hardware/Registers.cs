using System;

using static DotNetGB.Util.BitUtils;

namespace DotNetGB.Hardware
{
    public class Registers
    {
        public Flags Flags { get; } = new Flags();

        public int A { get; set; }

        public int B { get; set; }

        public int C { get; set; }

        public int D { get; set; }

        public int E { get; set; }

        public int H { get; set; }

        public int L { get; set; }

        public int AF
        {
            get => A << 8 | Flags.FlagsByte;
            set
            {
                CheckWordArgument("af", value);
                A = GetMSB(value);
                Flags.FlagsByte = GetLSB(value);
            }
        }

        public int BC
        {
            get => B << 8 | C;
            set
            {
                CheckWordArgument("bc", value);
                B = GetMSB(value);
                C = GetLSB(value);
            }
        }

        public int DE
        {
            get => D << 8 | E;
            set
            {
                CheckWordArgument("de", value);
                D = GetMSB(value);
                E = GetLSB(value);
            }
        }

        public int HL
        {
            get => H << 8 | L;
            set
            {
                CheckWordArgument("hl", value);
                H = GetMSB(value);
                L = GetLSB(value);
            }
        }

        public int PC { get; set; }

        public int SP { get; set; }

        public void IncrementPC() => PC = (PC + 1) & 0xffff;

        public void DecrementSP() => SP = (SP - 1) & 0xffff;

        public void IncrementSP() => SP = (SP + 1) & 0xffff;

        public override string ToString() => $"AF={AF:x4}, BC={BC:x4}, DE={DE:x4}, HL={HL:x4}, SP={SP:x4}, PC={PC:x4}, {Flags}";
    }
}