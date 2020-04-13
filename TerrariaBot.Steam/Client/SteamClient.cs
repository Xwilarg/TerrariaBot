using Steamworks;
using System;
using System.IO;
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

        public void ConnectWithSteamId(ulong friendSteamId, PlayerInformation playerInfos, string serverPassword = "", PlayerStartModifier? modifier = null)
        {
            Steamworks.SteamClient.Init(terrariaSteamId);
            _friendSteamId = new SteamId() { Value = friendSteamId };
            SteamNetworking.OnP2PSessionRequest = (steamid) =>
            {
                LogDebug(steamid + " is requesting to send P2P packet");
                if (steamid == _friendSteamId.Value)
                {
                    SteamNetworking.AcceptP2PSessionWithUser(steamid);
                }
            };
            InitPlayerInfos(playerInfos, serverPassword, modifier);
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
