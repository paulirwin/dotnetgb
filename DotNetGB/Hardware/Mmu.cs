using System.Collections.Generic;

using static DotNetGB.Util.BitUtils;

namespace DotNetGB.Hardware
{
    public class Mmu : IAddressSpace
    {
        private class VoidAddressSpace : IAddressSpace
        {
            public bool Accepts(int address) => true;

            public int this[int address]
            {
                get => 0xff;
                set
                {
                }
            }
        }

        private static readonly IAddressSpace VOID = new VoidAddressSpace();

        private readonly IList<IAddressSpace> _spaces = new List<IAddressSpace>();

        public void AddAddressSpace(IAddressSpace addressSpace) => _spaces.Add(addressSpace);

        public bool Accepts(int address) => true;

        public int this[int address]
        {
            get
            {
                CheckWordArgument("address", address);
                return GetSpace(address)[address];
            }
            set
            {
                CheckByteArgument("value", value);
                CheckWordArgument("address", address);
                GetSpace(address)[address] = value;
            }
        }

        private IAddressSpace GetSpace(int address)
        {
            foreach (var s in _spaces)
            {
                if (s.Accepts(address))
                {
                    return s;
                }
            }

            return VOID;
        }
    }
}
