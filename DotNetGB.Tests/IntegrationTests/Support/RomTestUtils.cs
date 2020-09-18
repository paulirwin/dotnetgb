using System;
using System.IO;
using Xunit;

namespace DotNetGB.Tests.IntegrationTests.Support
{
    public static class RomTestUtils
    {
        public static void TestRomWithMemory(FileInfo romPath)
        {
            Console.Out.WriteLine($"\n### Running test rom {romPath} ###");
            var runner = new MemoryTestRunner(romPath, Console.Out);
            var result = runner.RunTest();
            Assert.Equal(0, result.Status);
        }

        public static void TestRomWithSerial(FileInfo romPath)
        {
            Console.Out.WriteLine($"\n### Running test rom {romPath} ###");
            var runner = new SerialTestRunner(romPath, Console.Out);
            string result = runner.RunTest();
            Assert.Contains("Passed", result);
        }

        public static void TestMooneyeRom(FileInfo romPath)
        {
            Console.Out.WriteLine($"\n### Running test rom {romPath} ###");
            var runner = new MooneyeTestRunner(romPath, Console.Out);
            Assert.True(runner.RunTest());
        }
    }
}