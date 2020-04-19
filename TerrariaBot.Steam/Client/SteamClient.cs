using System.IO;
using TerrariaBot.Client;
using Steamworks;
using System;
using System.Linq;

namespace TerrariaBot.Steam.Client
{
    public class SteamClient : AClient
    {
        public SteamClient() : base()
        {
            if (!File.Exists("steam_appid.txt"))
                File.WriteAllText("steam_appid.txt", terrariaSteamId.ToString());
            _friendSteamId = null;
            _sessionConnectFail = Callback<P2PSessionConnectFail_t>.Create(OnP2PSessionConnectFail);
            if (SteamAPI.RestartAppIfNecessary(new AppId_t(terrariaSteamId)))
                throw new System.Exception("RestartAppIfNecessary returned false");
            if (!SteamAPI.Init())
            {
                LogError("Error while initializing Steamworks service, make sure you are connected to Steam and you own Terraria.");
            }
            LogDebug("Packsize test:" + Packsize.Test());
            LogDebug("DllCheck test:" + DllCheck.Test());
        }

        ~SteamClient()
        {
            SteamAPI.Shutdown();
        }

        public void ConnectWithSteamId(ulong friendSteamId, PlayerInformation playerInfos, string serverPassword = "")
        {
            _friendSteamId = new CSteamID(friendSteamId);
            SteamNetworking.AllowP2PPacketRelay(true);
            SendAuthTicket();
            P2PSessionState_t state;
            while (SteamNetworking.GetP2PSessionState(_friendSteamId.Value, out state) && state.m_bConnectionActive != 1)
            { }
            InitPlayerInfos(playerInfos, serverPassword);
        }

        private void SendAuthTicket()
        {
            byte[] auth = new byte[1021];
            uint dataLength;
            SteamUser.GetAuthSessionTicket(auth, auth.Length, out dataLength);
            uint finalLength = dataLength + 3U;
            byte[] request = new byte[finalLength];
            request[0] = (byte)(finalLength & 255);
            request[1] = (byte)(finalLength >> 8 & 255);
            request[2] = 93;
            for (int i = 0; i < dataLength; i++)
                request[i + 3] = auth[i];
            SteamNetworking.SendP2PPacket(_friendSteamId.Value, request, finalLength, EP2PSend.k_EP2PSendReliable, writeChannel);
        }

        protected override byte[] ReadMessage()
        {
            SteamAPI.RunCallbacks();
            byte[] buffer = new byte[2096];
            uint size;
            CSteamID steamId;
            var isValid = SteamNetworking.ReadP2PPacket(buffer, (uint)buffer.Length, out size, out steamId, readChannel);
            if (isValid)
            {
                if (steamId == _friendSteamId)
                {
                    byte[] finalBuffer = new byte[size - 2];
                    Array.Copy(buffer.Skip(2).ToArray(), finalBuffer, size - 2);
                    return finalBuffer;
                }
            }
            return new byte[0];
        }

        protected override void SendMessage(byte[] message)
        {
            if (!_friendSteamId.HasValue)
                 throw new NullReferenceException("You must call ConnectWithSteamId() before doing bot requests");
            bool sent = SteamNetworking.SendP2PPacket(_friendSteamId.Value, message, (uint)message.Length, EP2PSend.k_EP2PSendReliable, writeChannel);
            if (!sent)
                 LogError("Error while sending P2P packet");
        }

        void OnP2PSessionConnectFail(P2PSessionConnectFail_t error)
        {
            LogError("P2P session failed: " + error.m_eP2PSessionError);
        }

        private CSteamID? _friendSteamId;

        protected Callback<P2PSessionRequest_t> _sessionRequest;
        protected Callback<P2PSessionConnectFail_t> _sessionConnectFail;
        protected Callback<SocketStatusCallback_t> _socketStatusCallback;

        private const uint terrariaSteamId = 105600;
        private const int readChannel = 2;
        private const int writeChannel = 1;
    }
}
