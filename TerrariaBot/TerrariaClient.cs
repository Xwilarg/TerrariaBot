using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace TerrariaBot
{
    public class TerrariaClient
    {
        public TerrariaClient(string ip)
        {
            _client = new TcpClient(ip, 7777);
            _ns = _client.GetStream();
            _listenThread = new Thread(new ThreadStart(Listen));
            SendMessage(1, version);
        }

        ~TerrariaClient()
        {
            _client.Close();
        }

        public void Listen()
        {
            while (Thread.CurrentThread.IsAlive)
            {
                if (_ns.DataAvailable)
                {
                    lock (_ns)
                    {
                        BinaryReader reader = new BinaryReader(_ns);
                        NetworkMessage result = new NetworkMessage();
                        result.length = reader.ReadUInt16();
                        result.type = reader.ReadByte();
                        result.payload = reader.ReadString();
                        Console.WriteLine("Message received: " + result.type);
                    }
                }
            }
        }

        public void SendMessage(byte type, string payload)
        {
            lock (_ns)
            {
                BinaryWriter writer = new BinaryWriter(_ns);
                writer.Write((ushort)payload.Length);
                writer.Write(type);
                writer.Write(payload);
            }
        }

        private TcpClient _client;
        private NetworkStream _ns;
        private Thread _listenThread;

        private const string version = "Terraria194";
    }
}
