using System;

namespace DotNetGB.Hardware.GpuPhases
{
    public class IntQueue
    {
        private readonly int[] _array;

        private int _size;

        private int _offset = 0;

        public IntQueue(int capacity)
        {
            _array = new int[capacity];
            _size = 0;
            _offset = 0;
        }

        public int Size => _size;

        public void Enqueue(int value)
        {
            if (_size == _array.Length)
            {
                throw new InvalidOperationException("Queue is full");
            }
            _array[(_offset + _size) % _array.Length] = value;
            _size++;
        }

        public int Dequeue()
        {
            if (_size == 0)
            {
                throw new InvalidOperationException("Queue is empty");
            }
            _size--;
            int value = _array[_offset++];
            if (_offset == _array.Length)
            {
                _offset = 0;
            }
            return value;
        }

        public int Get(int index)
        {
            if (index >= _size)
            {
                throw new ArgumentOutOfRangeException();
            }
            return _array[(_offset + index) % _array.Length];
        }

        public void Set(int index, int value)
        {
            if (index >= _size)
            {
                throw new ArgumentOutOfRangeException();
            }
            _array[(_offset + index) % _array.Length] = value;
        }

        public void Clear()
        {
            _size = 0;
            _offset = 0;
        }
    }

}