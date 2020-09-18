using System.IO;
using Xunit;

using static DotNetGB.Tests.IntegrationTests.Support.RomTestUtils;

namespace DotNetGB.Tests.IntegrationTests.Blargg
{
    public class BlarggRomTests
    {
        [Fact]
        public void TestCgbSound() => TestRomWithMemory(GetPath("cgb_sound.gb"));

        [Fact]
        public void TestCpuInstrs() => TestRomWithSerial(GetPath("cpu_instrs.gb"));

        [Fact]
        public void TestDmgSound2() => TestRomWithMemory(GetPath("dmg_sound-2.gb"));

        [Fact]
        public void TestHaltBug() => TestRomWithMemory(GetPath("halt_bug.gb"));

        [Fact]
        public void TestInstrTiming() => TestRomWithSerial(GetPath("instr_timing.gb"));

        [Fact]
        public void TestInterruptTime() => TestRomWithMemory(GetPath("mem_timing-2.gb"));

        [Fact]
        public void TestOamBug2() => TestRomWithMemory(GetPath("oam_bug-2.gb"));

        private static FileInfo GetPath(string name) => new FileInfo(Path.Combine("Resources", "Blargg", name));
    }
}
