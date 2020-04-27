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
        private void UpdatePosition()
        {
            if (_velocity.X != 0 || _velocity.Y != 0)
            {
                double ms = DateTime.Now.Subtract(_dt).TotalMilliseconds / 20.0;
                _position += _velocity * (float)ms;
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
