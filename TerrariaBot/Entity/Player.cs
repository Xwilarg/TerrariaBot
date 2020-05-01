using System;
using System.Numerics;
using TerrariaBot.Client;

namespace TerrariaBot.Entity
{
    public class Player
    {
        public Player(AClient client, string name, byte slot)
        {
            _client = client;
            _name = name;
            _slot = slot;
            _position = Vector2.Zero;
            _velocity = Vector2.Zero;
        }

        public byte GetSlot() => _slot;
        public Vector2 GetPosition()
        {
            UpdatePosition();
            return _position;
        }
        internal void SetPosition(Vector2 pos, Vector2 vel)
        {
            UpdatePosition();
            _position = pos;
            _velocity = vel;
        }
        private void UpdateYPosition()
        {
            var blocSize = BlocGroup.blocPixelSize;
            var tile = _client.GetTile((int)_position.X / blocSize, (int)_position.Y / blocSize);
            if (BlocGroup.IsSolid(tile.GetTileType())) // The player is inside a tile
                _position.Y--;
            else
            {
                tile = _client.GetTile((int)_position.X / blocSize, ((int)_position.Y / blocSize) + 1);
                if (!BlocGroup.IsSolid(tile.GetTileType())) // Empty tile under the player
                    _position.Y++;
            }
        }
        private void UpdatePosition()
        {
            if (_velocity.X != 0 || _velocity.Y != 0)
            {
                double ms = DateTime.Now.Subtract(_dt).TotalMilliseconds / 20.0;
                var basePos = _position;
                _position += _velocity * (float)ms;
                var blocSize = BlocGroup.blocPixelSize;
                if (_position.X > basePos.X)
                {
                    for (int i = (int)basePos.X; i <= (int)_position.X + blocSize; i += blocSize)
                    {
                        UpdateYPosition();
                        var tile = _client.GetTile(i / blocSize, (int)_position.Y / blocSize);
                        if (BlocGroup.IsSolid(tile.GetTileType()))
                        {
                            _position = new Vector2(i - (blocSize * 2), _position.Y);
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = (int)basePos.X; i >= (int)_position.X + blocSize; i -= blocSize)
                    {
                        var tile = _client.GetTile(i / blocSize, (int)_position.Y / blocSize);
                        if (BlocGroup.IsSolid(tile.GetTileType()))
                        {
                            _position = new Vector2(i, _position.Y);
                            break;
                        }
                    }
                }
            }
            _dt = DateTime.Now;
        }
        public string GetName() => _name;

        protected AClient _client;
        protected readonly byte _slot;
        protected Vector2 _position, _velocity;
        private readonly string _name;
        private DateTime _dt;
    }
}
