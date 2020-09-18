using DotNetGB.Hardware;
using DotNetGB.Hardware.CpuOpcodes;
using Xunit;

namespace DotNetGB.Tests.CpuTests
{
    public class TimingTests
    {
        private const int OFFSET = 0x100;

        private readonly Cpu cpu;

        private readonly IAddressSpace memory;

        public TimingTests()
        {
            memory = new Ram(0x00, 0x10000);
            cpu = new Cpu(memory, new InterruptManager(false), null, new NullDisplay(), new SpeedMode());
        }

        [Fact]
        public void TestTiming()
        {
            AssertTiming(16, 0xc9, 0, 0); // RET
            AssertTiming(16, 0xd9, 0, 0); // RETI
            cpu.Registers.Flags.Z = false;
            AssertTiming(20, 0xc0, 0, 0); // RET NZ
            cpu.Registers.Flags.Z = true;
            AssertTiming(8, 0xc0, 0, 0); // RET NZ
            AssertTiming(24, 0xcd, 0, 0); // CALL a16
            AssertTiming(16, 0xc5); // PUSH BC
            AssertTiming(12, 0xf1); // POP AF

            AssertTiming(8, 0xd6, 00); // SUB A,d8

            cpu.Registers.Flags.C = true;
            AssertTiming(8, 0x30, 00); // JR nc,r8

            cpu.Registers.Flags.C = false;
            AssertTiming(12, 0x30, 00); // JR nc,r8

            cpu.Registers.Flags.C = true;
            AssertTiming(12, 0xd2, 00); // JP nc,a16

            cpu.Registers.Flags.C = false;
            AssertTiming(16, 0xd2, 00); // JP nc,a16

            AssertTiming(16, 0xc3, 00, 00); // JP a16

            AssertTiming(4, 0xaf); // XOR a
            AssertTiming(12, 0xe0, 0x05); // LD (ff00+05),A
            AssertTiming(12, 0xf0, 0x05); // LD A,(ff00+05)
            AssertTiming(4, 0xb7); // OR

            AssertTiming(4, 0x7b); // LDA A,E
            AssertTiming(8, 0xd6, 0x00); // SUB A,d8
            AssertTiming(8, 0xcb, 0x12); // RL D
            AssertTiming(4, 0x87); // ADD A
            AssertTiming(4, 0xf3); // DI
            AssertTiming(8, 0x32); // LD (HL-),A
            AssertTiming(12, 0x36); // LD (HL),d8
            AssertTiming(16, 0xea, 0x00, 0x00); // LD (a16),A
            AssertTiming(8, 0x09); // ADD HL,BC
            AssertTiming(16, 0xc7); // RST 00H


            AssertTiming(8, 0x3e, 0x51); // LDA A,51
            AssertTiming(4, 0x1f); // RRA
            AssertTiming(8, 0xce, 0x01); // ADC A,01
            AssertTiming(4, 0x00); // NOP
        }

        private void AssertTiming(int expectedTiming, params int[] opcodes)
        {
            for (int i = 0; i < opcodes.Length; i++)
            {
                memory[OFFSET + i] = opcodes[i];
            }

            cpu.ClearState();
            cpu.Registers.PC = OFFSET;

            int ticks = 0;
            Opcode opcode = null;
            do
            {
                cpu.Tick();
                if (opcode == null && cpu.CurrentOpcode != null)
                {
                    opcode = cpu.CurrentOpcode;
                }

                ticks++;
            } while (cpu.State != Cpu.CpuState.OPCODE || ticks < 4);

            Assert.Equal(expectedTiming, ticks);
        }
    }
}