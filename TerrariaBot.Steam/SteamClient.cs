using Steamworks;
using Steamworks.Data;
using System;
using System.Linq;
using System.Net;

namespace TerrariaBot.Steam
{
    public class SteamClient : TerrariaClient
    {
        public SteamClient(LogLevel logLevel) : base(logLevel)
        { }

        ~SteamClient()
        {
            Steamworks.SteamClient.Shutdown();
        }

        public void ConnectWithSteamId(ulong friendSteamId, PlayerInformation playerInfos, string serverPassword = "", PlayerStartModifier? modifier = null)
        {
            Steamworks.SteamClient.Init(105600);
            SteamFriends.OnGameLobbyJoinRequested += (lobby, id) => { OnGameLobbyJoinRequested(lobby, id, playerInfos, serverPassword, modifier); };
            if (!SteamFriends.GetFriends().Any(x => x.Id == friendSteamId))
                throw new ArgumentException("There is nobody in your friend list with this ID");
            _friendSteamId = friendSteamId;
            LogInfo("Waiting for a friend request from " + friendSteamId);
            /* byte[] writeInfos = new byte[1024];
             SteamNetworking.SendP2PPacket(new SteamId()
             {
                 Value = steamId
             }, writeInfos);
             */
        }

        private void OnGameLobbyJoinRequested(Lobby lobby, SteamId id, PlayerInformation playerInfos, string serverPassword, PlayerStartModifier? modifier)
        {
            LogDebug("Received a join request from " + id.Value + " for lobby " + lobby.Id);
            if (_friendSteamId == id.Value)
            {
                RoomEnter room = lobby.Join().GetAwaiter().GetResult();
                if (room != RoomEnter.Success)
                    LogError("Can't join friend game: " + room.ToString());
                else
                {
                    uint ip = 0;
                    ushort port = 0;
                    SteamId serverId = new SteamId(); ;
                    if (!lobby.GetGameServer(ref ip, ref port, ref serverId))
                    {
                        LogError("Can't get game server information");
                        lobby.Leave();
                    }
                    else
                    {
                        LogDebug("Lobby is hosted on IP " + ip + " with port " + port);
                        //Connect(, playerInfos, serverPassword, modifier);
                    } 
                }
            }
        }

        private ulong _friendSteamId;

        private const uint terrariaSteamId = 105600;
    }
}
