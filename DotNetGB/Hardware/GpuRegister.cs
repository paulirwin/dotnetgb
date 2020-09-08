namespace DotNetGB.Hardware
{
    public struct GpuRegister : MemoryRegisters.IRegister
    {
        private static readonly MemoryRegisters.RegisterType R = MemoryRegisters.RegisterType.R;
        private static readonly MemoryRegisters.RegisterType W = MemoryRegisters.RegisterType.W;
        private static readonly MemoryRegisters.RegisterType RW = MemoryRegisters.RegisterType.RW;

        public static readonly GpuRegister STAT = new GpuRegister(0, 0xff41, RW);
        public static readonly GpuRegister SCY = new GpuRegister(1, 0xff42, RW);
        public static readonly GpuRegister SCX = new GpuRegister(2, 0xff43, RW);
        public static readonly GpuRegister LY = new GpuRegister(3, 0xff44, R);
        public static readonly GpuRegister LYC = new GpuRegister(4, 0xff45, RW);
        public static readonly GpuRegister BGP = new GpuRegister(5, 0xff47, RW);
        public static readonly GpuRegister OBP0 = new GpuRegister(6, 0xff48, RW);
        public static readonly GpuRegister OBP1 = new GpuRegister(7, 0xff49, RW);
        public static readonly GpuRegister WY = new GpuRegister(8, 0xff4a, RW);
        public static readonly GpuRegister WX = new GpuRegister(9, 0xff4b, RW);
        public static readonly GpuRegister VBK = new GpuRegister(10, 0xff4f, W);

        public static GpuRegister[] Values { get; } = 
        {
            STAT,
            SCY,
            SCX,
            LY,
            LYC,
            BGP,
            OBP0,
            OBP1,
            WY,
            WX,
            VBK,
        };

        public int Address { get; }

        public MemoryRegisters.RegisterType Type { get; }

        public int Ordinal { get; set; }

        private GpuRegister(int ordinal, int address, MemoryRegisters.RegisterType type)
        {
            Ordinal = ordinal;
            Address = address;
            Type = type;
        }
    }
}
