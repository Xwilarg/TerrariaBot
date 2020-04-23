using System.Linq;
using System.Numerics;
using TerrariaBot.Client;

namespace TerrariaBot.Entity
{
    public class PlayerSelf : Player
    {
        public PlayerSelf(AClient client, string name, byte slot) : base(client, name, slot)
        { }

        public void DoAction(params PlayerAction[] actions)
        {
            float xVel = 0f;
            if (actions.Contains(PlayerAction.Left))
                xVel -= 3f;
            if (actions.Contains(PlayerAction.Right))
                xVel += 3f;
            _client.SendPlayerControls(this, (byte)actions.Sum(x => (int)x), xVel, 0f);
        }

        /// <summary>
        /// Teleport to a location
        /// </summary>
        public void Teleport(float x, float y)
        {
            _client.SendTeleport(this, x, y);
        }

        /// <summary>
        /// Teleport to a location
        /// </summary>
        public void Teleport(Vector2 pos)
        {
            _client.SendTeleport(this, pos.X, pos.Y);
        }

        /// <summary>
        /// Join or leave a team
        /// </summary>
        public void JoinTeam(Team teamId)
        {
            _client.JoinTeam(this, teamId);
        }

        /// <summary>
        /// Enable or disable PvP
        /// </summary>
        public void TogglePVP(bool status)
        {
            _client.TogglePVP(this, status);
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
