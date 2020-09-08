using System;
using System.Collections.Generic;
using DotNetGB.Util;

namespace DotNetGB.Hardware.CpuOpcodes
{
    public class AluFunctions
    {
        private readonly IDictionary<FunctionKey, IntRegistryFunction> _functions = new Dictionary<FunctionKey, IntRegistryFunction>();

        private readonly IDictionary<FunctionKey, BiIntRegistryFunction> _biFunctions = new Dictionary<FunctionKey, BiIntRegistryFunction>();

        public IntRegistryFunction FindAluFunction(string name, DataType argumentType)
        {
            return _functions[new FunctionKey(name, argumentType)];
        }

        public BiIntRegistryFunction FindAluFunction(string name, DataType arg1Type, DataType arg2Type)
        {
            return _biFunctions[new FunctionKey(name, arg1Type, arg2Type)];
        }

        private void RegisterAluFunction(string name, DataType dataType, IntRegistryFunction function)
        {
            _functions[new FunctionKey(name, dataType)] = function;
        }

        private void RegisterAluFunction(string name, DataType dataType1, DataType dataType2, BiIntRegistryFunction function)
        {
            _biFunctions[new FunctionKey(name, dataType1, dataType2)] = function;
        }

        public AluFunctions()
        {
            RegisterAluFunction("INC", DataType.D8, (flags, arg) => {
                int result = (arg + 1) & 0xff;
                flags.Z = result == 0;
                flags.N = false;
                flags.H = (arg & 0x0f) == 0x0f;
                return result;
            });
            RegisterAluFunction("INC", DataType.D16, (flags, arg) => (arg + 1) & 0xffff);
            RegisterAluFunction("DEC", DataType.D8, (flags, arg) => {
                int result = (arg - 1) & 0xff;
                flags.Z = result == 0;
                flags.N = true;
                flags.H = (arg & 0x0f) == 0x0;
                return result;
            });
            RegisterAluFunction("DEC", DataType.D16, (flags, arg) => (arg - 1) & 0xffff);
            RegisterAluFunction("ADD", DataType.D16, DataType.D16, (flags, arg1, arg2) => {
                flags.N = false;
                flags.H = (arg1 & 0x0fff) + (arg2 & 0x0fff) > 0x0fff;
                flags.C = arg1 + arg2 > 0xffff;
                return (arg1 + arg2) & 0xffff;
            });
            RegisterAluFunction("ADD", DataType.D16, DataType.R8, (flags, arg1, arg2) => (arg1 + arg2) & 0xffff);
            RegisterAluFunction("ADD_SP", DataType.D16, DataType.R8, (flags, arg1, arg2) => {
                flags.Z = false;
                flags.N = false;

                int result = arg1 + arg2;
                flags.C = (((arg1 & 0xff) + (arg2 & 0xff)) & 0x100) != 0;
                flags.H = (((arg1 & 0x0f) + (arg2 & 0x0f)) & 0x10) != 0;
                return result & 0xffff;
            });
            RegisterAluFunction("DAA", DataType.D8, (flags, arg) => {
                int result = arg;
                if (flags.N)
                {
                    if (flags.H)
                    {
                        result = (result - 6) & 0xff;
                    }
                    if (flags.C)
                    {
                        result = (result - 0x60) & 0xff;
                    }
                }
                else
                {
                    if (flags.H || (result & 0xf) > 9)
                    {
                        result += 0x06;
                    }
                    if (flags.C || result > 0x9f)
                    {
                        result += 0x60;
                    }
                }
                flags.H = false;
                if (result > 0xff)
                {
                    flags.C = true;
                }
                result &= 0xff;
                flags.Z = result == 0;
                return result;
            });
            RegisterAluFunction("CPL", DataType.D8, (flags, arg) => {
                flags.N = true;
                flags.H = true;
                return (~arg) & 0xff;
            });
            RegisterAluFunction("SCF", DataType.D8, (flags, arg) => {
                flags.N = false;
                flags.H = false;
                flags.C = true;
                return arg;
            });
            RegisterAluFunction("CCF", DataType.D8, (flags, arg) => {
                flags.N = false;
                flags.H = false;
                flags.C = !flags.C;
                return arg;
            });
            RegisterAluFunction("ADD", DataType.D8, DataType.D8, (flags, byte1, byte2) => {
                flags.Z = ((byte1 + byte2) & 0xff) == 0;
                flags.N = false;
                flags.H = (byte1 & 0x0f) + (byte2 & 0x0f) > 0x0f;
                flags.C = byte1 + byte2 > 0xff;
                return (byte1 + byte2) & 0xff;
            });
            RegisterAluFunction("ADC", DataType.D8, DataType.D8, (flags, byte1, byte2) => {
                int carry = flags.C ? 1 : 0;
                flags.Z = ((byte1 + byte2 + carry) & 0xff) == 0;
                flags.N = false;
                flags.H = (byte1 & 0x0f) + (byte2 & 0x0f) + carry > 0x0f;
                flags.C = byte1 + byte2 + carry > 0xff;
                return (byte1 + byte2 + carry) & 0xff;
            });
            RegisterAluFunction("SUB", DataType.D8, DataType.D8, (flags, byte1, byte2) => {
                flags.Z = ((byte1 - byte2) & 0xff) == 0;
                flags.N = true;
                flags.H = (0x0f & byte2) > (0x0f & byte1);
                flags.C = byte2 > byte1;
                return (byte1 - byte2) & 0xff;
            });
            RegisterAluFunction("SBC", DataType.D8, DataType.D8, (flags, byte1, byte2) => {
                int carry = flags.C ? 1 : 0;
                int res = byte1 - byte2 - carry;

                flags.Z = (res & 0xff) == 0;
                flags.N = true;
                flags.H = ((byte1 ^ byte2 ^ (res & 0xff)) & (1 << 4)) != 0;
                flags.C = res < 0;
                return res & 0xff;
            });
            RegisterAluFunction("AND", DataType.D8, DataType.D8, (flags, byte1, byte2) => {
                int result = byte1 & byte2;
                flags.Z = result == 0;
                flags.N = false;
                flags.H = true;
                flags.C = false;
                return result;
            });
            RegisterAluFunction("OR", DataType.D8, DataType.D8, (flags, byte1, byte2) => {
                int result = byte1 | byte2;
                flags.Z = result == 0;
                flags.N = false;
                flags.H = false;
                flags.C = false;
                return result;
            });
            RegisterAluFunction("XOR", DataType.D8, DataType.D8, (flags, byte1, byte2) => {
                int result = (byte1 ^ byte2) & 0xff;
                flags.Z = result == 0;
                flags.N = false;
                flags.H = false;
                flags.C = false;
                return result;
            });
            RegisterAluFunction("CP", DataType.D8, DataType.D8, (flags, byte1, byte2) => {
                flags.Z = ((byte1 - byte2) & 0xff) == 0;
                flags.N = true;
                flags.H = (0x0f & byte2) > (0x0f & byte1);
                flags.C = byte2 > byte1;
                return byte1;
            });
            RegisterAluFunction("RLC", DataType.D8, (flags, arg) => {
                int result = (arg << 1) & 0xff;
                if ((arg & (1 << 7)) != 0)
                {
                    result |= 1;
                    flags.C = true;
                }
                else
                {
                    flags.C = false;
                }
                flags.Z = result == 0;
                flags.N = false;
                flags.H = false;
                return result;
            });
            RegisterAluFunction("RRC", DataType.D8, (flags, arg) => {
                int result = arg >> 1;
                if ((arg & 1) == 1)
                {
                    result |= (1 << 7);
                    flags.C = true;
                }
                else
                {
                    flags.C = false;
                }
                flags.Z = result == 0;
                flags.N = false;
                flags.H = false;
                return result;
            });
            RegisterAluFunction("RL", DataType.D8, (flags, arg) => {
                int result = (arg << 1) & 0xff;
                result |= flags.C ? 1 : 0;
                flags.C = (arg & (1 << 7)) != 0;
                flags.Z = result == 0;
                flags.N = false;
                flags.H = false;
                return result;
            });
            RegisterAluFunction("RR", DataType.D8, (flags, arg) => {
                int result = arg >> 1;
                result |= flags.C ? (1 << 7) : 0;
                flags.C = (arg & 1) != 0;
                flags.Z = result == 0;
                flags.N = false;
                flags.H = false;
                return result;
            });
            RegisterAluFunction("SLA", DataType.D8, (flags, arg) => {
                int result = (arg << 1) & 0xff;
                flags.C = (arg & (1 << 7)) != 0;
                flags.Z = result == 0;
                flags.N = false;
                flags.H = false;
                return result;
            });
            RegisterAluFunction("SRA", DataType.D8, (flags, arg) => {
                int result = (arg >> 1) | (arg & (1 << 7));
                flags.C = (arg & 1) != 0;
                flags.Z = result == 0;
                flags.N = false;
                flags.H = false;
                return result;
            });
            RegisterAluFunction("SWAP", DataType.D8, (flags, arg) => {
                int upper = arg & 0xf0;
                int lower = arg & 0x0f;
                int result = (lower << 4) | (upper >> 4);
                flags.Z = result == 0;
                flags.N = false;
                flags.H = false;
                flags.C = false;
                return result;
            });
            RegisterAluFunction("SRL", DataType.D8, (flags, arg) => {
                int result = (arg >> 1);
                flags.C = (arg & 1) != 0;
                flags.Z = result == 0;
                flags.N = false;
                flags.H = false;
                return result;
            });
            RegisterAluFunction("BIT", DataType.D8, DataType.D8, (flags, arg1, arg2) => {
                int bit = arg2;
                flags.N = false;
                flags.H = true;
                if (bit < 8)
                {
                    flags.Z = !BitUtils.GetBit(arg1, arg2);
                }
                return arg1;
            });
            RegisterAluFunction("RES", DataType.D8, DataType.D8, (flags, arg1, arg2) =>BitUtils.ClearBit(arg1, arg2));
            RegisterAluFunction("SET", DataType.D8, DataType.D8, (flags, arg1, arg2) =>BitUtils.SetBit(arg1, arg2));
        }

        public delegate int IntRegistryFunction(Flags flags, int arg);

        public delegate int BiIntRegistryFunction(Flags flags, int arg1, int arg2);
        
        private readonly struct FunctionKey
        {
            private readonly string _name;

            private readonly DataType _type1;

            private readonly DataType? _type2;

            public FunctionKey(string name, DataType type1, DataType? type2 = null)
            {
                _name = name;
                _type1 = type1;
                _type2 = type2;
            }

            private bool Equals(FunctionKey other)
            {
                return _name == other._name && _type1 == other._type1 && _type2 == other._type2;
            }

            public override bool Equals(object? obj)
            {
                return obj is FunctionKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(_name, (int) _type1, _type2);
            }
        }
    }
}