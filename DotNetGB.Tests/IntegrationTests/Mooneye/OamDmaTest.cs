﻿using System.Collections.Generic;
using System.IO;
using DotNetGB.Tests.IntegrationTests.Support;
using Xunit;

namespace DotNetGB.Tests.IntegrationTests.Mooneye
{
    public class OamDmaTest
    {
        [Theory]
        [MemberData(nameof(Data))]
        public void Test(FileInfo romPath) => RomTestUtils.TestMooneyeRom(romPath);

        public static IEnumerable<object[]> Data => ParametersProvider.GetParameters(Path.Combine("Mooneye", "acceptance", "oam_dma"));
    }
}
