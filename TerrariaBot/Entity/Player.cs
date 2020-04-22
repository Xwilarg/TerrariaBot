using System.Numerics;
using TerrariaBot.Client;

namespace TerrariaBot.Entity
{
    public class Player
    {
        public Player(AClient client, byte slot)
        {
            _client = client;
            _slot = slot;
        }

        public byte GetSlot() => _slot;
        public Vector2 GetPosition() => _position; internal void SetPosition(Vector2 value) => _position = value;

        protected AClient _client;
        protected byte _slot;
        private Vector2 _position;
    }
}
