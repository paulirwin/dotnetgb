﻿using System;

namespace DotNetGB.Hardware
{
    public class ShadowAddressSpace : IAddressSpace
    {
        private readonly IAddressSpace _addressSpace;

        private readonly int _echoStart;

        private readonly int _targetStart;

        private readonly int _length;

        public ShadowAddressSpace(IAddressSpace addressSpace, int echoStart, int targetStart, int length)
        {
            _addressSpace = addressSpace;
            _echoStart = echoStart;
            _targetStart = targetStart;
            _length = length;
        }

        public bool Accepts(int address) => address >= _echoStart && address < _echoStart + _length;

        public int this[int address]
        {
            get => _addressSpace[Translate(address)];
            set => _addressSpace[Translate(address)] = value;
        }

        private int Translate(int address) => GetRelative(address) + _targetStart;

        private int GetRelative(int address)
        {
            int i = address - _echoStart;
            if (i < 0 || i >= _length)
            {
                throw new ArgumentOutOfRangeException();
            }
            return i;
        }
    }
}