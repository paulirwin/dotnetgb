
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static System.Diagnostics.Debug;

namespace DotNetGB.Util
{
    public static class BitUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMSB(int word)
        {
            CheckWordArgument("word", word);
            return word >> 8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetLSB(int word)
        {
            CheckWordArgument("word", word);
            return word & 0xff;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToWord(int[] bytes)
        {
            return ToWord(bytes[1], bytes[0]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToWord(int msb, int lsb)
        {
            CheckByteArgument("msb", msb);
            CheckByteArgument("lsb", lsb);
            return (msb << 8) | lsb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetBit(int byteValue, int position)
        {
            return (byteValue & (1 << position)) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SetBit(int byteValue, int position, bool value)
        {
            return value ? SetBit(byteValue, position) : ClearBit(byteValue, position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SetBit(int byteValue, int position)
        {
            CheckByteArgument("byteValue", byteValue);
            return (byteValue | (1 << position)) & 0xff;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ClearBit(int byteValue, int position)
        {
            CheckByteArgument("byteValue", byteValue);
            return ~(1 << position) & byteValue & 0xff;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToSigned(int byteValue)
        {
            if ((byteValue & (1 << 7)) == 0)
            {
                return byteValue;
            }

            return byteValue - 0x100;
        }

        [Conditional("DEBUG")]
        public static void CheckByteArgument(string argumentName, int argument)
        {
            Assert(argument >= 0 && argument <= 0xff, $"Argument {argumentName} should be a byte");
        }

        [Conditional("DEBUG")]
        public static void CheckWordArgument(string argumentName, int argument)
        {
            Assert(argument >= 0 && argument <= 0xffff, $"Argument {argumentName} should be a word");
        }
    }
}
