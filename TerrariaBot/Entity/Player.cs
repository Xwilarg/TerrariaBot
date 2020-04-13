namespace TerrariaBot.Entity
{
    public class Player
    {
        public Player(TerrariaClient client, byte slot)
        {
            _client = client;
            _slot = slot;
        }

        public byte GetSlot() => _slot;

        protected TerrariaClient _client;
        protected byte _slot;
    }
}
