using System.Text;
using static DotNetGB.Util.BitUtils;

namespace DotNetGB.Hardware
{
    public class Flags
    {
        private const int Z_POS = 7;

        private const int N_POS = 6;

        private const int H_POS = 5;

        private const int C_POS = 4;

        private int _flags;

        public int FlagsByte
        {
            get => _flags;
            set
            {
                CheckByteArgument("flags", value);
                _flags = value & 0xf0;
            }
        }

        public bool Z
        {
            get => GetBit(_flags, Z_POS);
            set => _flags = SetBit(_flags, Z_POS, value);
        }

        public bool N
        {
            get => GetBit(_flags, N_POS);
            set => _flags = SetBit(_flags, N_POS, value);
        }

        public bool H
        {
            get => GetBit(_flags, H_POS);
            set => _flags = SetBit(_flags, H_POS, value);
        }

        public bool C
        {
            get => GetBit(_flags, C_POS);
            set => _flags = SetBit(_flags, C_POS, value);
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            result.Append(Z ? 'Z' : '-');
            result.Append(N ? 'N' : '-');
            result.Append(H ? 'H' : '-');
            result.Append(C ? 'C' : '-');
            result.Append("----");
            return result.ToString();
        }
    }
}