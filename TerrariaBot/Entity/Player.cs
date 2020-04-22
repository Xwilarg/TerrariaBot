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
        }

        public byte GetSlot() => _slot;
        public Vector2 GetPosition() => _position; internal void SetPosition(Vector2 value) => _position = value;
        public string GetName() => _name;

        protected AClient _client;
        protected readonly byte _slot;
        private Vector2 _position;
        private readonly string _name;
    }
}
