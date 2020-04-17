using TerrariaBot.Client;

namespace TerrariaBot.Entity
{
    public class PlayerSelf : Player
    {
        public PlayerSelf(AClient client, byte slot) : base(client, slot)
        { }

        public void TogglePVP(bool status)
        {
            ushort length = 2;
            var writer = _client.WriteHeader(length, NetworkRequest.TogglePVP);
            writer.Write(_slot);
            writer.Write((byte)(status ? 1 : 0));
            _client.SendWrittenBytes();
        }

        public void JoinTeam(Team teamId)
        {
            ushort length = 2;
            var writer = _client.WriteHeader(length, NetworkRequest.JoinTeam);
            writer.Write(_slot);
            writer.Write((byte)teamId);
            _client.SendWrittenBytes();
        }
        public void SendChatMessage(string message)
        {
            ushort length = 0;
            var writer = _client.WriteHeader(length, NetworkRequest.ChatMessage);
            writer.Write((ushort)1);
            writer.Write(_client.GetName());
            writer.Write(message);
            _client.SendWrittenBytes();
        }
    }
}
