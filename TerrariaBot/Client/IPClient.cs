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

        internal override byte[] ReadMessage(out NetworkRequest messageType)
        {
            byte[] buf = new byte[2]; // contains length (uint16)
            _ns.Read(buf, 0, 2);
            // Length contains the length of the length (2 octets), the type (1 octet) and the payload
            // We remove 3 to only keep the length of the payload
            int length = BitConverter.ToUInt16(buf) - 3;
            buf = new byte[1]; // contains type (uint8)
            _ns.Read(buf, 0, 1);
            messageType = (NetworkRequest)buf[0];
            byte[] payload;
            if (length > 0)
            {
                payload = new byte[length];
                _ns.Read(payload, 0, payload.Length);
            }
            else
                payload = new byte[0];
            return payload;
        }

        internal override BinaryWriter SendMessage(ushort length, NetworkRequest type)
        {
            if (_client == null)
                throw new NullReferenceException("You must call ConnectWithIP() before doing bot requests");
            BinaryWriter writer = new BinaryWriter(_ns);
            writer.Write((ushort)(length + 3));
            writer.Write((byte)type);
            return writer;
        }

        private TcpClient _client;
        private NetworkStream _ns;
    }
}
