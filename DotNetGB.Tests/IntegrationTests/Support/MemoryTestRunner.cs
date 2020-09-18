using System;
using System.IO;
using System.Text;
using DotNetGB.Hardware;

namespace DotNetGB.Tests.IntegrationTests.Support
{
    public class MemoryTestRunner
    {
        private readonly Gameboy gb;

        private readonly StringBuilder text;

        private readonly TextWriter os;

        private bool testStarted;

        public MemoryTestRunner(FileInfo romFile, TextWriter os)
        {
            var options = new GameboyOptions(romFile);
            var cart = new Cartridge(options);
            gb = new Gameboy(options, cart, new NullDisplay(), new NullController(), new NullSoundOutput(), new NullSerialEndpoint());
            text = new StringBuilder();
            this.os = os;
        }

        public TestResult RunTest()
        {
            int status = 0x80;
            int divider = 0;
            while (status == 0x80 && !SerialTestRunner.IsInfiniteLoop(gb))
            {
                gb.Tick();
                if (++divider >= (gb.SpeedMode.Mode == 2 ? 1 : 4))
                {
                    status = GetTestResult(gb);
                    divider = 0;
                }
            }

            return new TestResult(status, text.ToString());
        }

        private int GetTestResult(Gameboy gb)
        {
            var mem = gb.AddressSpace;
            if (!testStarted)
            {
                int i = 0xa000;
                foreach (int v in new[] {0x80, 0xde, 0xb0, 0x61} ) {
                    if (mem[i++] != v)
                    {
                        return 0x80;
                    }
                }
                testStarted = true;
            }

            int status = mem[0xa000];

            if (gb.Cpu.State != Cpu.CpuState.OPCODE)
            {
                return status;
            }

            var reg = gb.Cpu.Registers;
            int pc = reg.PC;
            foreach (int v in new[] {0xe5, 0xf5, 0xfa, 0x83, 0xd8}) {
                if (mem[pc++] != v)
                {
                    return status;
                }
            }
            char c = (char) reg.A;
            text.Append(c);
            os?.Write(c);

            reg.PC = reg.PC + 0x19;
            return status;
        }

        public class TestResult
        {
            public TestResult(int status, string text)
            {
                Status = status;
                Text = text;
            }

            public int Status { get; }

            public string Text { get; }

            public override string ToString() => $"{Status:X2}: {Text}";
        }
    }
}