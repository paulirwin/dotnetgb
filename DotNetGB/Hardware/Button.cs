using System;

namespace DotNetGB.Hardware
{
    public readonly struct Button
    {
        public static readonly Button RIGHT = new Button(0x01, 0x10);
        public static readonly Button LEFT = new Button(0x02, 0x10);
        public static readonly Button UP = new Button(0x04, 0x10);
        public static readonly Button DOWN = new Button(0x08, 0x10);
        public static readonly Button A = new Button(0x01, 0x20);
        public static readonly Button B = new Button(0x02, 0x20);
        public static readonly Button SELECT = new Button(0x04, 0x20);
        public static readonly Button START = new Button(0x08, 0x20);

        private Button(int mask, int line)
        {
            Mask = mask;
            Line = line;
        }
        
        public int Mask { get; }

        public int Line { get; }
    }
}
