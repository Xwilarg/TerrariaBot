using TerrariaBot.Client;

namespace TerrariaBot.Entity
{
    public class PlayerSelf : Player
    {
        public PlayerSelf(AClient client, byte slot) : base(client, slot)
        { }

        /// <summary>
        /// Enable or disable PvP
        /// </summary>
        public void TogglePVP(bool status)
        {
            ushort length = 2;
            var writer = _client.WriteHeader(length, NetworkRequest.TogglePVP);
            writer.Write(_slot);
            writer.Write((byte)(status ? 1 : 0));
            _client.SendWrittenBytes();
        }

        /// <summary>
        /// Join or leave a team
        /// </summary>
        public void JoinTeam(Team teamId)
        {
            ushort length = 2;
            var writer = _client.WriteHeader(length, NetworkRequest.JoinTeam);
            writer.Write(_slot);
            writer.Write((byte)teamId);
            _client.SendWrittenBytes();
        }

        /// <summary>
        /// Send a message using the in-game chat
        /// </summary>
        public void SendChatMessage(string message)
        {
            ushort length = (ushort)(2 + _client.GetName().Length + message.Length + 2);
            var writer = _client.WriteHeader(length, NetworkRequest.ChatMessage);
            writer.Write((ushort)1);
            writer.Write(_client.GetName());
            writer.Write(message);
            _client.SendWrittenBytes();
        }
    }
}
