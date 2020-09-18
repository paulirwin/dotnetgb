using System.Collections.Generic;
using System.IO;
using DotNetGB.Hardware;

namespace DotNetGB.Tests.IntegrationTests.Support
{
    public class MooneyeTestRunner
    {
        private readonly Gameboy gb;

        private readonly Cpu cpu;

        private readonly IAddressSpace mem;

        private readonly Registers regs;

        private readonly TextWriter os;

        public MooneyeTestRunner(FileInfo romFile, TextWriter os)
        {
            var opts = new List<string>();
            if (romFile.Name.EndsWith("-C.gb") || romFile.Name.EndsWith("-cgb.gb"))
            {
                opts.Add("c");
            }
            if (romFile.Name.StartsWith("boot_"))
            {
                opts.Add("b");
            }
            opts.Add("db");
            var options = new GameboyOptions(romFile, new List<string>(), opts);
            var cart = new Cartridge(options);
            gb = new Gameboy(options, cart, new NullDisplay(), new NullController(), new NullSoundOutput(), new NullSerialEndpoint());
            os.WriteLine("System type: " + (cart.IsGbc ? "CGB" : "DMG"));
            os.WriteLine("Bootstrap: " + (options.UseBootstrap ? "enabled" : "disabled"));
            cpu = gb.Cpu;
            regs = cpu.Registers;
            mem = gb.AddressSpace;
            this.os = os;
        }

        public bool RunTest()
        {
            int divider = 0;
            while (!IsByteSequenceAtPc(0x00, 0x18, 0xfd)) // infinite loop
            { 
                gb.Tick();
                if (++divider >= (gb.SpeedMode.Mode == 2 ? 1 : 4))
                {
                    DisplayProgress();
                    divider = 0;
                }
            }
            return regs.A == 0 && regs.B == 3 && regs.C == 5 && regs.D == 8 && regs.E == 13 && regs.H == 21 && regs.L == 34;
        }

        private void DisplayProgress()
        {
            if (cpu.State == Cpu.CpuState.OPCODE && mem[regs.PC] == 0x22 && regs.HL >= 0x9800 && regs.HL < 0x9c00)
            {
                if (regs.A != 0)
                {
                    os.Write(regs.A);
                }
            }
            else if (IsByteSequenceAtPc(0x7d, 0xe6, 0x1f, 0xee, 0x1f))
            {
                os.Write('\n');
            }
        }

        private bool IsByteSequenceAtPc(params int[] seq)
        {
            if (cpu.State != Cpu.CpuState.OPCODE)
            {
                return false;
            }

            int i = regs.PC;
            bool found = true;
            foreach (int v in seq)
            {
                if (mem[i++] != v)
                {
                    found = false;
                    break;
                }
            }
            return found;
        }
    }
}
