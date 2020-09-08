using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DotNetGB.Hardware.CpuOpcodes
{
    public class Opcode
    {
        public int OpcodeValue { get; }

        public string Label { get; }

        public IReadOnlyList<IOp> Ops { get; }

        public int OperandLength { get; }

        public Opcode(OpcodeBuilder builder)
        {
            OpcodeValue = builder.Opcode;
            Label = builder.Label;
            Ops = builder.Ops.ToImmutableList();
            OperandLength = Ops.Count == 0 ? 0 : Ops.Select(i => i.OperandLength).Max();
        }

        public override string ToString() => $"{OpcodeValue:x2} {Label}";
    }
}
