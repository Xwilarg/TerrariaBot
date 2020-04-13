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

        protected AClient _client;
        protected byte _slot;
    }
}
