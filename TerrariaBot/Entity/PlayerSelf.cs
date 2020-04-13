namespace TerrariaBot.Entity
{
    public class PlayerSelf : Player
    {
        public PlayerSelf(TerrariaClient client, byte slot) : base(client, slot)
        { }

        public void TogglePVP(bool status)
        {
            ushort length = 2;
            var writer = _client.SendMessage(length, NetworkRequest.TogglePVP);
            writer.Write(_slot);
            writer.Write((byte)(status ? 1 : 0));
            writer.Flush();
        }

        public void JoinTeam(Team teamId)
        {
            ushort length = 2;
            var writer = _client.SendMessage(length, NetworkRequest.JoinTeam);
            writer.Write(_slot);
            writer.Write((byte)teamId);
            writer.Flush();
        }
    }
}
