using System.Collections.Generic;
using DotNetGB.Hardware;

namespace DotNetGB.Util
{
    public static class GpuModeExtensions
    {
        private static readonly List<Gpu.GpuMode> _gpuModeOrdinals = new List<Gpu.GpuMode>()
        {
            Gpu.GpuMode.HBlank,
            Gpu.GpuMode.VBlank,
            Gpu.GpuMode.OamSearch,
            Gpu.GpuMode.PixelTransfer,
        };

        public static int Ordinal(this Gpu.GpuMode type) => _gpuModeOrdinals.IndexOf(type);
    }
}
