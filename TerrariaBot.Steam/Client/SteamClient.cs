using Steamworks;
using System;
using System.IO;
using TerrariaBot.Client;

namespace TerrariaBot.Steam.Client
{/*
    public sealed class SteamClient : AClient
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
            InitPlayerInfos(playerInfos, serverPassword, modifier);
        }

        internal override byte[] ReadMessage(out NetworkRequest messageType)
        {
            throw new NotImplementedException();
        }

        internal override BinaryWriter SendMessage(ushort length, NetworkRequest type)
        {
            throw new NotImplementedException();
        }

        private SteamId? _friendSteamId;

        private const uint terrariaSteamId = 105600;
    }*/
}
