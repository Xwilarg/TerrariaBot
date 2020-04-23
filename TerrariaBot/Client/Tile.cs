namespace TerrariaBot.Client
{
    internal class Tile
    {
        public Tile()
        {
            _type = 0;
            _wall = 0;
            _liquid = 0;
            _sTileHeader = 0;
            _bTileHeader = 0;
            _bTileHeader2 = 0;
            _bTileHeader3 = 0;
            _frameX = 0;
            _frameY = 0;
        }

        public Tile(byte id)
        {
            _type = id;
            _wall = 0;
            _liquid = 0;
            _sTileHeader = 0;
            _bTileHeader = 0;
            _bTileHeader2 = 0;
            _bTileHeader3 = 0;
            _frameX = 0;
            _frameY = 0;
        }

        public Tile(Tile t) // Copy ctor, we asume t isn't null
        {
            _type = t._type;
            _wall = t._wall;
            _liquid = t._liquid;
            _sTileHeader = t._sTileHeader;
            _bTileHeader = t._bTileHeader;
            _bTileHeader2 = t._bTileHeader2;
            _bTileHeader3 = t._bTileHeader3;
            _frameX = t._frameX;
            _frameY = t._frameY;
        }

        public bool IsActive()
            => (_sTileHeader & 32) == 32;

        public void Activate(bool value)
        {
            if (value)
                _sTileHeader |= 32;
            else
                _sTileHeader = (short)(_sTileHeader & 65503);
        }

        public void Color(byte color)
        {
            if (color > 30) color = 30;
            _sTileHeader = (short)((_sTileHeader & 65504) | color);
        }

        public void ColorWall(byte color)
        {
            if (color > 30) color = 30;
            _bTileHeader = (short)((_bTileHeader & 224) | color);
        }

        private ushort _type; public ushort GetTileType() => _type; public void SetTileType(ushort value) => _type = value;
        private byte _wall; public void SetWall(byte value) => _wall = value;
        private byte _liquid; public byte GetLiquid() => _liquid; public void SetLiquid(byte value) => _liquid = value;
        private short _sTileHeader;
        private short _bTileHeader;
        private short _bTileHeader2;
        private short _bTileHeader3;
        private short _frameX;
        private short _frameY;
        public void SetFrames(short x, short y)
        {
            _frameX = x;
            _frameY = y;
        }
    }
}
