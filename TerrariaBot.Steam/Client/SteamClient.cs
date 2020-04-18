using System.IO;
using TerrariaBot.Client;
using Steamworks;
using System;

namespace TerrariaBot.Steam.Client
{
    public class SteamClient : AClient
    {
        public SteamClient() : base()
        {
            if (!File.Exists("steam_appid.txt"))
                File.WriteAllText("steam_appid.txt", terrariaSteamId.ToString());
            _friendSteamId = null;
            _sessionRequest = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);
            _sessionConnectFail = Callback<P2PSessionConnectFail_t>.Create(OnP2PSessionConnectFail);
            _socketStatusCallback = Callback<SocketStatusCallback_t>.Create(OnSocketStatusCallback);
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
            InitPlayerInfos(playerInfos, serverPassword);
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
                    LogDebug("Packet size: " + size);
                    return buffer;
                }
                else
                    LogDebug("Ignoring packet from " + steamId);
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


        void OnP2PSessionRequest(P2PSessionRequest_t session)
        {
            LogDebug("Received session request from " + session.m_steamIDRemote);

            if (!SteamNetworking.AcceptP2PSessionWithUser(session.m_steamIDRemote))
                LogError("Failed to accept P2P session with " + session.m_steamIDRemote);
        }

        void OnP2PSessionConnectFail(P2PSessionConnectFail_t error)
        {
            LogError("P2P session failed: " + error.m_eP2PSessionError);
        }

        void OnSocketStatusCallback(SocketStatusCallback_t status)
        {
            LogDebug("Session status update: " + status.m_eSNetSocketState);
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
