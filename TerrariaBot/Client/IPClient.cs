using System;
using System.Net.Sockets;

namespace TerrariaBot.Client
{
    public sealed class IPClient : AClient
    {
        public IPClient() : base()
        { }

        ~IPClient()
        {
            _ns.Close();
            _client.Close();
        }

        public void ConnectWithIP(string ip, PlayerInformation playerInfos, string serverPassword = "")
        {
            _client = new TcpClient(ip, 7777);
            _ns = _client.GetStream();
            InitPlayerInfos(playerInfos, serverPassword);
        }

        protected override byte[] ReadMessage()
        {
            byte[] buf = new byte[2]; // contains length (uint16)
            _ns.Read(buf, 0, 2);
            // Length contains the length of the length (2 octets)
            int length = BitConverter.ToUInt16(buf) - 2;
            if (length <= 0)
                return new byte[0];
            buf = new byte[length];
            _ns.Read(buf, 0, length);
            return buf;
        }

        protected override void SendMessage(byte[] message)
        {
            if (_client == null)
                throw new NullReferenceException("You must call ConnectWithIP() before doing bot requests");
            _ns.Write(message);
            _ns.Flush();
        }

        private TcpClient _client;
        private NetworkStream _ns;
    }
}
