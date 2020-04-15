using Steamworks;
using System;
using System.Linq;
using TerrariaBot.Client;

namespace TerrariaBot.Steam.Client
{
    public class SteamClient : AClient
    {
        public SteamClient(LogLevel logLevel) : base(logLevel)
        {
            _friendSteamId = null;
        }

        ~SteamClient()
        {
            Steamworks.SteamClient.Shutdown();
        }

        public void ConnectWithSteamId(ulong friendSteamId, PlayerInformation playerInfos, string serverPassword = "")
        {
            try
            {
                Steamworks.SteamClient.Init(terrariaSteamId);
            }
            catch (System.Exception e)
            {
                throw new System.Exception("Steam inilization failed. Make sure you are logged in your Steam account and you have Terraria.");
            }
            var friend = SteamFriends.GetFriends().Where(x => x.Id == friendSteamId).FirstOrDefault();
            if (!friend.Id.IsValid)
                throw new ArgumentException("You don't have any friend with this id");
            LogInfo("Attempting to connect to " + friend.Name);
            _friendSteamId = new SteamId() { Value = friendSteamId };
            SteamNetworking.OnP2PSessionRequest = (steamid) =>
            {
                LogDebug(steamid + " is requesting to send P2P packet");
                if (steamid == _friendSteamId.Value)
                {
                    SteamNetworking.AcceptP2PSessionWithUser(steamid);
                }
            };
            InitPlayerInfos(playerInfos, serverPassword);
        }

        protected override byte[] ReadMessage()
        {
            byte[] buffer = new byte[2096];
            uint size = 0;
            SteamId steamId = new SteamId();
            var isValid = SteamNetworking.ReadP2PPacket(buffer, ref size, ref steamId, readChannel);
            if (isValid)
            {
                if (steamId == _friendSteamId)
                {
                    LogDebug("Packet size: " + size);
                    return buffer;
                }
                else
                    LogDebug("Ignoring packet from " + steamId.Value);
            }
            return new byte[0];
        }

        protected override void SendMessage(byte[] message)
        {
            if (!_friendSteamId.HasValue)
                throw new NullReferenceException("You must call ConnectWithSteamId() before doing bot requests");
            bool sent = SteamNetworking.SendP2PPacket(_friendSteamId.Value, message, message.Length, writeChannel);
            if (!sent)
                LogError("Error while sending P2P packet");
        }

        private SteamId? _friendSteamId;

        private const uint terrariaSteamId = 105600;
        private const int readChannel = 2;
        private const int writeChannel = 1;
    }
}
