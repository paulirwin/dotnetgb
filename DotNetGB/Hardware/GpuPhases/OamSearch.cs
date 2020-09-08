namespace DotNetGB.Hardware.GpuPhases
{
    public class OamSearch : IGpuPhase
    {
        private enum State
        {
            READING_Y,
            READING_X,
        }

        public class SpritePosition
        {
            public SpritePosition(int x, int y, int address)
            {
                X = x;
                Y = y;
                Address = address;
            }

            public int X { get; }

            public int Y { get; }

            public int Address { get; }
        }

        private readonly IAddressSpace _oemRam;

        private readonly MemoryRegisters _registers;

        private readonly SpritePosition?[] _sprites;

        private readonly Lcdc _lcdc;

        private int _spritePosIndex;

        private State _state;

        private int _spriteY;

        private int _spriteX;

        private int _i;
        
        public OamSearch(IAddressSpace oemRam, Lcdc lcdc, MemoryRegisters registers)
        {
            _oemRam = oemRam;
            _registers = registers;
            _lcdc = lcdc;
            _sprites = new SpritePosition[10];
        }

        public OamSearch Start()
        {
            _spritePosIndex = 0;
            _state = State.READING_Y;
            _spriteY = 0;
            _spriteX = 0;
            _i = 0;
            for (int j = 0; j < _sprites.Length; j++)
            {
                _sprites[j] = null;
            }
            return this;
        }

        public bool Tick()
        {
            int spriteAddress = 0xfe00 + 4 * _i;
            switch (_state)
            {
                case State.READING_Y:
                    _spriteY = _oemRam[spriteAddress];
                    _state = State.READING_X;
                    break;

                case State.READING_X:
                    _spriteX = _oemRam[spriteAddress + 1];
                    if (_spritePosIndex < _sprites.Length && Between(_spriteY, _registers.Get(GpuRegister.LY) + 16, _spriteY + _lcdc.SpriteHeight))
                    {
                        _sprites[_spritePosIndex++] = new SpritePosition(_spriteX, _spriteY, spriteAddress);
                    }
                    _i++;
                    _state = State.READING_Y;
                    break;
            }
            return _i < 40;
        }

        public SpritePosition?[] Sprites => _sprites;

        private static bool Between(int from, int x, int to) => from <= x && x < to;
    }
}
