using System.IO;
using System.Text;
using DotNetGB.Hardware;

using static DotNetGB.Util.BitUtils;

namespace DotNetGB.Tests.IntegrationTests.Support
{
    public class SerialTestRunner : ISerialEndpoint
    {
        private readonly Gameboy gb;

        private readonly StringBuilder text;

        private readonly TextWriter os;

        public SerialTestRunner(FileInfo romFile, TextWriter os)
        {
            var options = new GameboyOptions(romFile);
            var cart = new Cartridge(options);
            gb = new Gameboy(options, cart, new NullDisplay(), new NullController(), new NullSoundOutput(), this);
            text = new StringBuilder();
            this.os = os;
        }

        public string RunTest()
        {
            int divider = 0;
            while (true)
            {
                gb.Tick();
                if (++divider == 4)
                {
                    if (IsInfiniteLoop(gb))
                    {
                        break;
                    }
                    divider = 0;
                }
            }
            return text.ToString();
        }

        public int Transfer(int outgoing)
        {
            text.Append((char)outgoing);
            os.Write(outgoing);
            os.Flush();
            return 0;
        }

        public static bool IsInfiniteLoop(Gameboy gb)
        {
            var cpu = gb.Cpu;
            if (cpu.State != Cpu.CpuState.OPCODE)
            {
                return false;
            }
            
            var regs = cpu.Registers;
            var mem = gb.AddressSpace;

            int i = regs.PC;
            bool found = true;
            foreach (int v in new[] { 0x18, 0xfe })
            { 
                // jr fe
                if (mem[i++] != v)
                {
                    found = false;
                    break;
                }
            }
            if (found)
            {
                return true;
            }

            i = regs.PC;
            foreach (int v in new[] { 0xc3, GetLSB(i), GetMSB(i) })
            { 
                // jp pc
                if (mem[i++] != v)
                {
                    return false;
                }
            }
            return true;
        }
    }
}