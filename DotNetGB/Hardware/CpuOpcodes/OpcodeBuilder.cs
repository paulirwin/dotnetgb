using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using DotNetGB.Util;
using static DotNetGB.Util.BitUtils;

namespace DotNetGB.Hardware.CpuOpcodes
{
    public class OpcodeBuilder
    {
        private static readonly AluFunctions ALU = new AluFunctions();
        private static readonly ISet<AluFunctions.IntRegistryFunction> OEM_BUG;

        static OpcodeBuilder()
        {
            var oemBugFunctions = new HashSet<AluFunctions.IntRegistryFunction>
            {
                ALU.FindAluFunction("INC", DataType.D16), 
                ALU.FindAluFunction("DEC", DataType.D16),
            };
            OEM_BUG = oemBugFunctions.ToImmutableHashSet();
        }

        private DataType lastDataType;

        public OpcodeBuilder(int opcode, string label)
        {
            Opcode = opcode;
            Label = label;
        }

        public OpcodeBuilder CopyByte(string target, string source)
        {
            Load(source);
            Store(target);
            return this;
        }

        public OpcodeBuilder Load(string source)
        {
            Argument arg = Argument.Parse(source);
            lastDataType = arg.DataType;
            Ops.Add(new LoadOp(arg));
            return this;
        }

        private sealed class LoadOp : IOp
        {
            private readonly Argument arg;

            public LoadOp(Argument arg)
            {
                this.arg = arg;
            }

            public bool ReadsMemory => arg.IsMemory;

            public int OperandLength => arg.OperandLength;

            public int Execute(Registers registers, IAddressSpace addressSpace, int[] args, int context)
            {
                return arg.Read(registers, addressSpace, args);
            }

            public override string ToString()
            {
                if (arg.DataType == DataType.D16)
                {
                    return $"{arg.Label} → [__]";
                }
                else
                {
                    return $"{arg.Label} → [_]";
                }
            }
        }

        public OpcodeBuilder LoadWord(int value)
        {
            lastDataType = DataType.D16;
            Ops.Add(new LoadWordOp(value));
            return this;
        }

        private sealed class LoadWordOp : IOp
        {
            private readonly int value;

            public LoadWordOp(int value)
            {
                this.value = value;
            }

            public int Execute(Registers registers, IAddressSpace addressSpace, int[] args, int context)
            {
                return value;
            }

            public override string ToString()
            {
                return $"0x{value:X2} → [__]";
            }
        }

        public OpcodeBuilder Store(string target)
        {
            Argument arg = Argument.Parse(target);

            if (lastDataType == DataType.D16 && arg == Argument._a16)
            {
                Ops.Add(new StoreD16Part1Op(arg));
                Ops.Add(new StoreD16Part2Op(arg));
            }
            else if (lastDataType == arg.DataType)
            {
                Ops.Add(new StoreOp(arg));
            }
            else
            {
                throw new InvalidOperationException("Can't write " + lastDataType + " to " + target);
            }

            return this;
        }

        private sealed class StoreD16Part1Op : IOp
        {
            private readonly Argument arg;

            public StoreD16Part1Op(Argument arg)
            {
                this.arg = arg;
            }

            public bool WritesMemory => arg.IsMemory;

            public int OperandLength => arg.OperandLength;

            public int Execute(Registers registers, IAddressSpace addressSpace, int[] args, int context)
            {
                addressSpace[ToWord(args)] = context & 0x00ff;
                return context;
            }

            public override string ToString()
            {
                return $"[ _] → {arg.Label}";
            }
        }

        private sealed class StoreD16Part2Op : IOp
        {
            private readonly Argument arg;

            public StoreD16Part2Op(Argument arg)
            {
                this.arg = arg;
            }

            public bool WritesMemory => arg.IsMemory;

            public int OperandLength => arg.OperandLength;

            public int Execute(Registers registers, IAddressSpace addressSpace, int[] args, int context)
            {
                addressSpace[(ToWord(args) + 1) & 0xffff] = (context & 0xff00) >> 8;
                return context;
            }

            public override string ToString()
            {
                return $"[_ ] → {arg.Label}";
            }
        }

        private sealed class StoreOp : IOp
        {
            private readonly Argument arg;

            public StoreOp(Argument arg)
            {
                this.arg = arg;
            }

            public bool WritesMemory => arg.IsMemory;

            public int OperandLength => arg.OperandLength;

            public int Execute(Registers registers, IAddressSpace addressSpace, int[] args, int context)
            {
                arg.Write(registers, addressSpace, args, context);
                return context;
            }

            public override string ToString()
            {
                if (arg.DataType == DataType.D16)
                {
                    return $"[__] → {arg.Label}";
                }
                else
                {
                    return $"[_] → {arg.Label}";
                }
            }
        }

        public OpcodeBuilder ProceedIf(string condition)
        {
            Ops.Add(new ProceedIfOp(condition));
            return this;
        }

        private sealed class ProceedIfOp : IOp
        {
            private readonly string condition;

            public ProceedIfOp(string condition)
            {
                this.condition = condition;
            }

            public bool Proceed(Registers registers)
            {
                switch (condition)
                {
                    case "NZ":
                        return !registers.Flags.Z;
                    case "Z":
                        return registers.Flags.Z;
                    case "NC":
                        return !registers.Flags.C;
                    case "C":
                        return registers.Flags.C;
                }

                return false;
            }

            public override string ToString()
            {
                return $"? {condition}:";
            }
        }

        public OpcodeBuilder Push()
        {
            AluFunctions.IntRegistryFunction dec = ALU.FindAluFunction("DEC", DataType.D16);
            Ops.Add(new PushPart1Op(dec));
            Ops.Add(new PushPart2Op(dec));
            return this;
        }

        private sealed class PushPart1Op : IOp
        {
            private readonly AluFunctions.IntRegistryFunction dec;

            public PushPart1Op(AluFunctions.IntRegistryFunction dec)
            {
                this.dec = dec;
            }

            public bool WritesMemory => true;

            public int Execute(Registers registers, IAddressSpace addressSpace, int[] args, int context)
            {
                registers.SP = dec(registers.Flags, registers.SP);
                addressSpace[registers.SP] = (context & 0xff00) >> 8;
                return context;
            }

            public SpriteBug.CorruptionType? CausesOemBug(Registers registers, int context)
            {
                return InOamArea(registers.SP) ? SpriteBug.CorruptionType.PUSH_1 : (SpriteBug.CorruptionType?)null;
            }

            public override string ToString()
            {
                return "[_ ] → (SP--)";
            }
        }

        private sealed class PushPart2Op : IOp
        {
            private readonly AluFunctions.IntRegistryFunction dec;

            public PushPart2Op(AluFunctions.IntRegistryFunction dec)
            {
                this.dec = dec;
            }

            public bool WritesMemory => true;

            public int Execute(Registers registers, IAddressSpace addressSpace, int[] args, int context)
            {
                registers.SP = dec(registers.Flags, registers.SP);
                addressSpace[registers.SP] = context & 0x00ff;
                return context;
            }

            public SpriteBug.CorruptionType? CausesOemBug(Registers registers, int context)
            {
                return InOamArea(registers.SP) ? SpriteBug.CorruptionType.PUSH_2 : (SpriteBug.CorruptionType?) null;
            }

            public override string ToString()
            {
                return "[ _] → (SP--)";
            }
        }

        public OpcodeBuilder Pop()
        {
            AluFunctions.IntRegistryFunction inc = ALU.FindAluFunction("INC", DataType.D16);
            lastDataType = DataType.D16;
            Ops.Add(new PopPart1Op(inc));
            Ops.Add(new PopPart2Op(inc));
            return this;
        }

        private sealed class PopPart1Op : IOp
        {
            private readonly AluFunctions.IntRegistryFunction inc;

            public PopPart1Op(AluFunctions.IntRegistryFunction inc)
            {
                this.inc = inc;
            }

            public bool ReadsMemory => true;

            public int Execute(Registers registers, IAddressSpace addressSpace, int[] args, int context)
            {
                int lsb = addressSpace[registers.SP];
                registers.SP = inc(registers.Flags, registers.SP);
                return lsb;
            }

            public SpriteBug.CorruptionType? CausesOemBug(Registers registers, int context)
            {
                return InOamArea(registers.SP) ? SpriteBug.CorruptionType.POP_1 : (SpriteBug.CorruptionType?) null;
            }

            public override string ToString()
            {
                return "(SP++) → [ _]";
            }
        }

        private sealed class PopPart2Op : IOp
        {
            private readonly AluFunctions.IntRegistryFunction inc;

            public PopPart2Op(AluFunctions.IntRegistryFunction inc)
            {
                this.inc = inc;
            }

            public bool ReadsMemory => true;

            public int Execute(Registers registers, IAddressSpace addressSpace, int[] args, int context)
            {
                int msb = addressSpace[registers.SP];
                registers.SP = inc(registers.Flags, registers.SP);
                return context | (msb << 8);
            }

            public SpriteBug.CorruptionType? CausesOemBug(Registers registers, int context)
            {
                return InOamArea(registers.SP) ? SpriteBug.CorruptionType.POP_2 : (SpriteBug.CorruptionType?) null;
            }

            public override string ToString()
            {
                return "(SP++) → [_ ]";
            }
        }

        public OpcodeBuilder Alu(string operation, string argument2)
        {
            Argument arg2 = Argument.Parse(argument2);
            AluFunctions.BiIntRegistryFunction func = ALU.FindAluFunction(operation, lastDataType, arg2.DataType);
            Ops.Add(new AluArg2Op(this, func, arg2, operation));
            if (lastDataType == DataType.D16)
            {
                ExtraCycle();
            }

            return this;
        }

        private sealed class AluArg2Op : IOp
        {
            private readonly OpcodeBuilder parent;
            private readonly AluFunctions.BiIntRegistryFunction func;
            private readonly Argument arg2;
            private readonly string operation;

            public AluArg2Op(OpcodeBuilder parent, AluFunctions.BiIntRegistryFunction func, Argument arg2, string operation)
            {
                this.func = func;
                this.arg2 = arg2;
                this.operation = operation;
                this.parent = parent;
            }

            public bool ReadsMemory => arg2.IsMemory;

            public int OperandLength => arg2.OperandLength;

            public int Execute(Registers registers, IAddressSpace addressSpace, int[] args, int v1)
            {
                int v2 = arg2.Read(registers, addressSpace, args);
                return func(registers.Flags, v1, v2);
            }

            public override string ToString()
            {
                if (parent.lastDataType == DataType.D16)
                {
                    return $"{operation}([__],{arg2}) → [__]";
                }
                else
                {
                    return $"{operation}([_],{arg2}) → [_]";
                }
            }
        }

        public OpcodeBuilder Alu(string operation, int d8Value)
        {
            AluFunctions.BiIntRegistryFunction func = ALU.FindAluFunction(operation, lastDataType, DataType.D8);
            Ops.Add(new AluD8Op(func, d8Value, operation));
            if (lastDataType == DataType.D16)
            {
                ExtraCycle();
            }

            return this;
        }

        private sealed class AluD8Op : IOp
        {
            private readonly AluFunctions.BiIntRegistryFunction func;
            private readonly int d8Value;
            private readonly string operation;

            public AluD8Op(AluFunctions.BiIntRegistryFunction func, int d8Value, string operation)
            {
                this.func = func;
                this.d8Value = d8Value;
                this.operation = operation;
            }

            public int Execute(Registers registers, IAddressSpace addressSpace, int[] args, int v1)
            {
                return func(registers.Flags, v1, d8Value);
            }

            public override string ToString()
            {
                return $"{operation}({d8Value},[_]) → [_]";
            }
        }

        public OpcodeBuilder Alu(string operation)
        {
            AluFunctions.IntRegistryFunction func = ALU.FindAluFunction(operation, lastDataType);
            Ops.Add(new AluOp(this, func, operation));
            if (lastDataType == DataType.D16)
            {
                ExtraCycle();
            }

            return this;
        }

        private sealed class AluOp : IOp
        {
            private readonly OpcodeBuilder parent;
            private readonly AluFunctions.IntRegistryFunction func;
            private readonly string operation;

            public AluOp(OpcodeBuilder parent, AluFunctions.IntRegistryFunction func, string operation)
            {
                this.parent = parent;
                this.func = func;
                this.operation = operation;
            }

            public int Execute(Registers registers, IAddressSpace addressSpace, int[] args, int value)
            {
                return func(registers.Flags, value);
            }

            public SpriteBug.CorruptionType? CausesOemBug(Registers registers, int context)
            {
                return OpcodeBuilder.CausesOemBug(func, context) ? SpriteBug.CorruptionType.INC_DEC : (SpriteBug.CorruptionType?) null;
            }

            public override string ToString()
            {
                if (parent.lastDataType == DataType.D16)
                {
                    return $"{operation}([__]) → [__]";
                }
                else
                {
                    return $"{operation}([_]) → [_]";
                }
            }
        }

        public OpcodeBuilder AluHL(string operation)
        {
            Load("HL");
            AluFunctions.IntRegistryFunction func = ALU.FindAluFunction(operation, DataType.D16);
            Ops.Add(new AluHLOp(func, operation));
            Store("HL");
            return this;
        }

        private sealed class AluHLOp : IOp
        {
            private readonly AluFunctions.IntRegistryFunction func;
            private readonly string operation;

            public AluHLOp(AluFunctions.IntRegistryFunction func, string operation)
            {
                this.func = func;
                this.operation = operation;
            }

            public int Execute(Registers registers, IAddressSpace addressSpace, int[] args, int value)
            {
                return func(registers.Flags, value);
            }

            public SpriteBug.CorruptionType? CausesOemBug(Registers registers, int context)
            {
                return OpcodeBuilder.CausesOemBug(func, context) ? SpriteBug.CorruptionType.LD_HL : (SpriteBug.CorruptionType?) null;
            }

            public override string ToString()
            {
                return $"{operation}(HL) → [__]";
            }
        }

        public OpcodeBuilder BitHL(int bit)
        {
            Ops.Add(new BitHLOp(bit));
            return this;
        }

        private sealed class BitHLOp : IOp
        {
            private readonly int bit;

            public BitHLOp(int bit)
            {
                this.bit = bit;
            }

            public bool ReadsMemory => true;

            public int Execute(Registers registers, IAddressSpace addressSpace, int[] args, int context)
            {
                int value = addressSpace[registers.HL];
                Flags flags = registers.Flags;
                flags.N = false;
                flags.H = true;
                if (bit < 8)
                {
                    flags.Z = !BitUtils.GetBit(value, bit);
                }

                return context;
            }

            public override string ToString()
            {
                return $"BIT({bit},HL)";
            }
        }

        public OpcodeBuilder ClearZ()
        {
            Ops.Add(new ClearZOp());
            return this;
        }

        private sealed class ClearZOp : IOp
        {
            public int Execute(Registers registers, IAddressSpace addressSpace, int[] args, int context)
            {
                registers.Flags.Z = false;
                return context;
            }

            public override string ToString()
            {
                return "0 → Z";
            }
        }

        public OpcodeBuilder SwitchInterrupts(bool enable, bool withDelay)
        {
            Ops.Add(new SwitchInterruptsOp(enable, withDelay));
            return this;
        }

        private sealed class SwitchInterruptsOp : IOp
        {
            private readonly bool enable;
            private readonly bool withDelay;

            public SwitchInterruptsOp(bool enable, bool withDelay)
            {
                this.enable = enable;
                this.withDelay = withDelay;
            }

            public void SwitchInterrupts(InterruptManager interruptManager)
            {
                if (enable)
                {
                    interruptManager.EnableInterrupts(withDelay);
                }
                else
                {
                    interruptManager.DisableInterrupts(withDelay);
                }
            }

            public override string ToString()
            {
                return (enable ? "enable" : "disable") + " interrupts";
            }
        }

        public OpcodeBuilder Op(IOp op)
        {
            Ops.Add(op);
            return this;
        }

        public OpcodeBuilder ExtraCycle()
        {
            Ops.Add(new ExtraCycleOp());
            return this;
        }

        private sealed class ExtraCycleOp : IOp
        {
            public bool ReadsMemory => true;

            public override string ToString()
            {
                return "wait cycle";
            }
        }

        public OpcodeBuilder ForceFinish()
        {
            Ops.Add(new ForceFinishOp());
            return this;
        }

        private sealed class ForceFinishOp : IOp
        {
            public bool ForceFinishCycle => true;

            public override string ToString()
            {
                return "finish cycle";
            }
        }

        public virtual Opcode Build()
        {
            return new Opcode(this);
        }

        public int Opcode { get; }

        public string Label { get; }

        public IList<IOp> Ops { get; } = new List<IOp>();

        public override string ToString()
        {
            return Label;
        }

        private static bool CausesOemBug(AluFunctions.IntRegistryFunction function, int context)
        {
            return OEM_BUG.Contains(function) && InOamArea(context);
        }

        private static bool InOamArea(int address)
        {
            return address >= 0xfe00 && address <= 0xfeff;
        }
    }
}