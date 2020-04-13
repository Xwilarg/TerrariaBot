using Steamworks;

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

        public void ConnectWithSteamId(ulong friendSteamId, string ip, PlayerInformation playerInfos, string serverPassword = "", PlayerStartModifier? modifier = null)
        {
            Steamworks.SteamClient.Init(105600);
            System.Console.WriteLine("Friend list:");
            foreach (var elem in SteamFriends.GetFriends())
            {
                System.Console.WriteLine(elem.Name);
            }
           /* byte[] writeInfos = new byte[1024];
            SteamNetworking.SendP2PPacket(new SteamId()
            {
                Value = steamId
            }, writeInfos);
            Connect(ip, playerInfos, serverPassword, modifier);*/
        }

        private const uint terrariaSteamId = 105600;
    }
}
