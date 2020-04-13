using System;
using System.IO;
using System.Net.Sockets;

namespace TerrariaBot.Client
{
    public sealed class IPClient : AClient
    {
        public IPClient(LogLevel logLevel) : base(logLevel)
        { }

        ~IPClient()
        {
            _ns.Close();
            _client.Close();
        }

        public void ConnectWithIP(string ip, PlayerInformation playerInfos, string serverPassword = "", PlayerStartModifier? modifier = null)
        {
            _client = new TcpClient(ip, 7777);
            _ns = _client.GetStream();
            InitPlayerInfos(playerInfos, serverPassword, modifier);
        }

        internal override byte[] ReadMessage()
        {
            byte[] buf = new byte[2]; // contains length (uint16)
            _ns.Read(buf, 0, 2);
            // Length contains the length of the length (2 octets)
            int length = BitConverter.ToUInt16(buf) - 2;
            buf = new byte[length];
            _ns.Read(buf, 0, length);
            return buf;
        }

        internal override void SendMessage(byte[] message)
        {
            if (_client == null)
                throw new NullReferenceException("You must call ConnectWithIP() before doing bot requests");
            BinaryWriter writer = new BinaryWriter(_ns);
            _ns.Write(message);
            _ns.Flush();
        }

        private TcpClient _client;
        private NetworkStream _ns;
    }
}
