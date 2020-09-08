using System;

namespace DotNetGB.Hardware
{
    public class DmaAddressSpace : IAddressSpace
    {
        private readonly IAddressSpace _addressSpace;

        public DmaAddressSpace(IAddressSpace addressSpace)
        {
            _addressSpace = addressSpace;
        }

        public bool Accepts(int address) => true;

        public int this[int address]
        {
            get => (address < 0xe000) ? _addressSpace[address] : _addressSpace[address - 0x2000];
            set => throw new InvalidOperationException();
        }
    }
}