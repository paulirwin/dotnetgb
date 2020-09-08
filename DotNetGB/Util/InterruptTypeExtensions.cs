using System.Collections.Generic;
using DotNetGB.Hardware;

namespace DotNetGB.Util
{
    public static class InterruptTypeExtensions
    {
        private static readonly List<InterruptManager.InterruptType> _interruptTypeOrdinals = new List<InterruptManager.InterruptType>
        {
            InterruptManager.InterruptType.VBlank,
            InterruptManager.InterruptType.LCDC,
            InterruptManager.InterruptType.Timer,
            InterruptManager.InterruptType.Serial,
            InterruptManager.InterruptType.P10_13,
        };

        public static int Ordinal(this InterruptManager.InterruptType type) => _interruptTypeOrdinals.IndexOf(type);
    }
}
