using System;
using DotNetGB.Util;
using static DotNetGB.Util.BitUtils;

namespace DotNetGB.Hardware.CpuOpcodes
{
    public abstract class Argument
    {
        public static readonly Argument A = new ArgumentA();
        public static readonly Argument B = new ArgumentB();
        public static readonly Argument C = new ArgumentC();
        public static readonly Argument D = new ArgumentD();
        public static readonly Argument E = new ArgumentE();
        public static readonly Argument H = new ArgumentH();
        public static readonly Argument L = new ArgumentL();
        public static readonly Argument AF = new ArgumentAF();
        public static readonly Argument BC = new ArgumentBC();
        public static readonly Argument DE = new ArgumentDE();
        public static readonly Argument HL = new ArgumentHL();
        public static readonly Argument SP = new ArgumentSP();
        public static readonly Argument PC = new ArgumentPC();
        public static readonly Argument d8 = new ArgumentD8();
        public static readonly Argument d16 = new ArgumentD16();
        public static readonly Argument r8 = new ArgumentR8();
        public static readonly Argument a16 = new ArgumentA16();
        public static readonly Argument _BC = new Argument_BC();
        public static readonly Argument _DE = new Argument_DE();
        public static readonly Argument _HL = new Argument_HL();
        public static readonly Argument _a8 = new Argument_A8();
        public static readonly Argument _a16 = new Argument_A16();
        public static readonly Argument _C = new Argument_C();

        public static readonly Argument[] Values =
        {
            A,
            B,
            C,
            D,
            E,
            H,
            L,
            AF,
            BC,
            DE,
            HL,
            SP,
            PC,
            d8,
            d16,
            r8,
            a16,
            _BC,
            _DE,
            _HL,
            _a8,
            _a16,
            _C,
        };

        public class ArgumentA : Argument
        {
            public ArgumentA() : base("A")
            {
            }

            public override int Read(Registers registers, IAddressSpace addressSpace, int[] args) => registers.A;

            public override void Write(Registers registers, IAddressSpace addressSpace, int[] args, int value) => registers.A = value;
        }

        public class ArgumentB : Argument
        {
            public ArgumentB() : base("B")
            {
            }

            public override int Read(Registers registers, IAddressSpace addressSpace, int[] args) => registers.B;

            public override void Write(Registers registers, IAddressSpace addressSpace, int[] args, int value) => registers.B = value;
        }

        public class ArgumentC : Argument
        {
            public ArgumentC() : base("C")
            {
            }

            public override int Read(Registers registers, IAddressSpace addressSpace, int[] args) => registers.C;

            public override void Write(Registers registers, IAddressSpace addressSpace, int[] args, int value) => registers.C = value;
        }

        public class ArgumentD : Argument
        {
            public ArgumentD() : base("D")
            {
            }

            public override int Read(Registers registers, IAddressSpace addressSpace, int[] args) => registers.D;

            public override void Write(Registers registers, IAddressSpace addressSpace, int[] args, int value) => registers.D = value;
        }

        public class ArgumentE : Argument
        {
            public ArgumentE() : base("E")
            {
            }

            public override int Read(Registers registers, IAddressSpace addressSpace, int[] args) => registers.E;

            public override void Write(Registers registers, IAddressSpace addressSpace, int[] args, int value) => registers.E = value;
        }

        public class ArgumentH : Argument
        {
            public ArgumentH() : base("H")
            {
            }

            public override int Read(Registers registers, IAddressSpace addressSpace, int[] args) => registers.H;

            public override void Write(Registers registers, IAddressSpace addressSpace, int[] args, int value) => registers.H = value;
        }

        public class ArgumentL : Argument
        {
            public ArgumentL() : base("L")
            {
            }

            public override int Read(Registers registers, IAddressSpace addressSpace, int[] args) => registers.L;

            public override void Write(Registers registers, IAddressSpace addressSpace, int[] args, int value) => registers.L = value;
        }

        public class ArgumentAF : Argument
        {
            public ArgumentAF() : base("AF", 0, false, DataType.D16)
            {
            }

            public override int Read(Registers registers, IAddressSpace addressSpace, int[] args) => registers.AF;

            public override void Write(Registers registers, IAddressSpace addressSpace, int[] args, int value) => registers.AF = value;
        }

        public class ArgumentBC : Argument
        {
            public ArgumentBC() : base("BC", 0, false, DataType.D16)
            {
            }

            public override int Read(Registers registers, IAddressSpace addressSpace, int[] args) => registers.BC;

            public override void Write(Registers registers, IAddressSpace addressSpace, int[] args, int value) => registers.BC = value;
        }

        public class ArgumentDE : Argument
        {
            public ArgumentDE() : base("DE", 0, false, DataType.D16)
            {
            }

            public override int Read(Registers registers, IAddressSpace addressSpace, int[] args) => registers.DE;

            public override void Write(Registers registers, IAddressSpace addressSpace, int[] args, int value) => registers.DE = value;
        }

        public class ArgumentHL : Argument
        {
            public ArgumentHL() : base("HL", 0, false, DataType.D16)
            {
            }

            public override int Read(Registers registers, IAddressSpace addressSpace, int[] args) => registers.HL;

            public override void Write(Registers registers, IAddressSpace addressSpace, int[] args, int value) => registers.HL = value;
        }

        public class ArgumentSP : Argument
        {
            public ArgumentSP() : base("SP", 0, false, DataType.D16)
            {
            }

            public override int Read(Registers registers, IAddressSpace addressSpace, int[] args) => registers.SP;

            public override void Write(Registers registers, IAddressSpace addressSpace, int[] args, int value) => registers.SP = value;
        }

        public class ArgumentPC : Argument
        {
            public ArgumentPC() : base("PC", 0, false, DataType.D16)
            {
            }

            public override int Read(Registers registers, IAddressSpace addressSpace, int[] args) => registers.PC;

            public override void Write(Registers registers, IAddressSpace addressSpace, int[] args, int value) => registers.PC = value;
        }

        public class ArgumentD8 : Argument
        {
            public ArgumentD8() : base("d8", 1, false, DataType.D8)
            {
            }

            public override int Read(Registers registers, IAddressSpace addressSpace, int[] args) => args[0];
        }

        public class ArgumentD16 : Argument
        {
            public ArgumentD16() : base("d16", 2, false, DataType.D16)
            {
            }

            public override int Read(Registers registers, IAddressSpace addressSpace, int[] args) => BitUtils.ToWord(args);
        }

        public class ArgumentR8 : Argument
        {
            public ArgumentR8() : base("r8", 1, false, DataType.R8)
            {
            }

            public override int Read(Registers registers, IAddressSpace addressSpace, int[] args) => ToSigned(args[0]);
        }

        public class ArgumentA16 : Argument
        {
            public ArgumentA16() : base("a16", 2, false, DataType.D16)
            {
            }

            public override int Read(Registers registers, IAddressSpace addressSpace, int[] args) => BitUtils.ToWord(args);
        }

        public class Argument_BC : Argument
        {
            public Argument_BC() : base("(BC)", 0, true, DataType.D8)
            {
            }

            public override int Read(Registers registers, IAddressSpace addressSpace, int[] args) => addressSpace[registers.BC];

            public override void Write(Registers registers, IAddressSpace addressSpace, int[] args, int value) => addressSpace[registers.BC] = value;
        }

        public class Argument_DE : Argument
        {
            public Argument_DE() : base("(DE)", 0, true, DataType.D8)
            {
            }

            public override int Read(Registers registers, IAddressSpace addressSpace, int[] args) => addressSpace[registers.DE];

            public override void Write(Registers registers, IAddressSpace addressSpace, int[] args, int value) => addressSpace[registers.DE] = value;
        }

        public class Argument_HL : Argument
        {
            public Argument_HL() : base("(HL)", 0, true, DataType.D8)
            {
            }

            public override int Read(Registers registers, IAddressSpace addressSpace, int[] args) => addressSpace[registers.HL];

            public override void Write(Registers registers, IAddressSpace addressSpace, int[] args, int value) => addressSpace[registers.HL] = value;
        }

        public class Argument_A8 : Argument
        {
            public Argument_A8() : base("(a8)", 1, true, DataType.D8)
            {
            }

            public override int Read(Registers registers, IAddressSpace addressSpace, int[] args) => addressSpace[0xff00 | args[0]];

            public override void Write(Registers registers, IAddressSpace addressSpace, int[] args, int value) => addressSpace[0xff00 | args[0]] = value;
        }

        public class Argument_A16 : Argument
        {
            public Argument_A16() : base("(a16)", 2, true, DataType.D8)
            {
            }

            public override int Read(Registers registers, IAddressSpace addressSpace, int[] args) => addressSpace[BitUtils.ToWord(args)];

            public override void Write(Registers registers, IAddressSpace addressSpace, int[] args, int value) => addressSpace[BitUtils.ToWord(args)] = value;
        }

        public class Argument_C : Argument
        {
            public Argument_C() : base("(C)", 0, true, DataType.D8)
            {
            }

            public override int Read(Registers registers, IAddressSpace addressSpace, int[] args) => addressSpace[0xff00 | registers.C];

            public override void Write(Registers registers, IAddressSpace addressSpace, int[] args, int value) => addressSpace[0xff00 | registers.C] = value;
        }

        private Argument(string label, int operandLength = 0, bool memory = false, DataType dataType = DataType.D8)
        {
            Label = label;
            OperandLength = operandLength;
            IsMemory = memory;
            DataType = dataType;
        }

        public int OperandLength { get; }

        public bool IsMemory { get; }

        public DataType DataType { get; }

        public string Label { get; }

        public abstract int Read(Registers registers, IAddressSpace addressSpace, int[] args);
        
        public virtual void Write(Registers registers, IAddressSpace addressSpace, int[] args, int value)
        {
            throw new NotSupportedException();
        }

        public static Argument Parse(string s)
        {
            foreach (var a in Values)
            {
                if (a.Label.Equals(s))
                {
                    return a;
                }
            }   
            throw new ArgumentException($"Unknown argument: {s}");
        }
    }
}
