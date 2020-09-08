using System;
using System.Text;

namespace DotNetGB.Hardware
{
    public class ColorPalette : IAddressSpace
    {
        private readonly int _indexAddr;

        private readonly int _dataAddr;

        private readonly int[][] _palettes =
        {
            new int[4],
            new int[4],
            new int[4],
            new int[4],
            new int[4],
            new int[4],
            new int[4],
            new int[4],
        };

        private int _index;

        private bool _autoIncrement;

        public ColorPalette(int offset)
        {
            _indexAddr = offset;
            _dataAddr = offset + 1;
        }

        public bool Accepts(int address) => address == _indexAddr || address == _dataAddr;

        public int this[int address]
        {
            get
            {
                if (address == _indexAddr)
                {
                    return _index | (_autoIncrement ? 0x80 : 0x00) | 0x40;
                }
                else if (address == _dataAddr)
                {
                    int color = _palettes[_index / 8][(_index % 8) / 2];
                    if (_index % 2 == 0)
                    {
                        return color & 0xff;
                    }
                    else
                    {
                        return (color >> 8) & 0xff;
                    }
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            set
            {
                if (address == _indexAddr)
                {
                    _index = value & 0x3f;
                    _autoIncrement = (value & (1 << 7)) != 0;
                }
                else if (address == _dataAddr)
                {
                    int color = _palettes[_index / 8][(_index % 8) / 2];
                    if (_index % 2 == 0)
                    {
                        color = (color & 0xff00) | value;
                    }
                    else
                    {
                        color = (color & 0x00ff) | (value << 8);
                    }
                    _palettes[_index / 8][(_index % 8) / 2] = color;
                    if (_autoIncrement)
                    {
                        _index = (_index + 1) & 0x3f;
                    }
                }
                else
                {
                    throw new ArgumentException();
                }
            }
        }

        public int[] GetPalette(int index) => _palettes[index];

        public override string ToString()
        {
            var b = new StringBuilder();
            for (int i = 0; i < 8; i++)
            {
                b.Append(i).Append(": ");
                int[] palette = GetPalette(i);
                foreach (int c in palette)
                {
                    b.Append(c.ToString("X4")).Append(' ');
                }
                b[^1] = '\n';
            }
            return b.ToString();
        }

        public void FillWithFF()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    _palettes[i][j] = 0x7fff;
                }
            }
        }
    }
}