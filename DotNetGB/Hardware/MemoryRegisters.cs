using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DotNetGB.Hardware
{
    public class MemoryRegisters : IAddressSpace
    {
        public interface IRegister
        {
            int Address { get; }

            RegisterType Type { get; }
        }

        public readonly struct RegisterType
        {
            public static readonly RegisterType R = new RegisterType(true, false);
            public static readonly RegisterType W = new RegisterType(false, true);
            public static readonly RegisterType RW = new RegisterType(true, true);

            public bool AllowsRead { get; }

            public bool AllowsWrite { get; }

            private RegisterType(bool allowsRead, bool allowsWrite)
            {
                AllowsRead = allowsRead;
                AllowsWrite = allowsWrite;
            }
        }

        private readonly IDictionary<int, IRegister> _registers;

        private readonly IDictionary<int, int> _values = new Dictionary<int, int>();

        public MemoryRegisters(IEnumerable<IRegister> registers)
        {
            var map = new Dictionary<int, IRegister>();
            foreach (var r in registers)
            {
                if (map.ContainsKey(r.Address))
                {
                    throw new ArgumentException($"Two registers with the same address: {r.Address}");
                }

                map[r.Address] = r;
                _values[r.Address] = 0;
            }

            _registers = map.ToImmutableDictionary();
        }

        private MemoryRegisters(MemoryRegisters original)
        {
            _registers = original._registers;
            _values = original._values.ToImmutableDictionary();
        }

        public int Get(IRegister reg)
        {
            // NOTE.PI: Removed "ContainsKey" check here because "else" just threw exception
            return _values[reg.Address];
        }

        public void Put(IRegister reg, int value)
        {
            // NOTE.PI: Removed "ContainsKey" check here because "else" just threw exception
            _values[reg.Address] = value;
        }

        public MemoryRegisters Freeze() => new MemoryRegisters(this);

        public int PreIncrement(IRegister reg)
        {
            if (_registers.ContainsKey(reg.Address))
            {
                int value = _values[reg.Address] + 1;
                _values[reg.Address] = value;
                return value;
            }
            else
            {
                throw new ArgumentException($"Not valid register: " + reg);
            }
        }

        public bool Accepts(int address) => _registers.ContainsKey(address);

        public int this[int address]
        {
            get
            {
                if (_registers[address].Type.AllowsRead)
                {
                    return _values[address];
                }
                else
                {
                    return 0xff;
                }
            }
            set
            {
                if (_registers[address].Type.AllowsWrite)
                {
                    _values[address] = value;
                }
            }
        }
    }
}