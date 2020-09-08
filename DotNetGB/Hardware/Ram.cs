using System;

namespace DotNetGB.Hardware
{
    public class Ram : IAddressSpace
    {
        private readonly int[] _space;

        private readonly int _length;

        private readonly int _offset;

        public Ram(int offset, int length)
        {
            _space = new int[length];
            _length = length;
            _offset = offset;
        }

        public Ram(int offset, int length, Ram ram)
        {
            _offset = offset;
            _length = length;
            _space = ram._space;
        }

        public static Ram CreateShadow(int offset, int length, Ram ram) => new Ram(offset, length, ram);

        public bool Accepts(int address) => address >= _offset && address < _offset + _length;

        public int this[int address]
        {
            get
            {
                int index = address - _offset;
                if (index < 0 || index >= _space.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(address), "Address: " + address);
                }
                return _space[index];
            }
            set => _space[address - _offset] = value;
        }
    }
}