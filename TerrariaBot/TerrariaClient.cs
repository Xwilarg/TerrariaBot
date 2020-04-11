using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TerrariaBot
{
    public class TerrariaClient
    {
        public TerrariaClient(string ip)
        {
            _slot = 0;

            _client = new TcpClient(ip, 7777);
            _ns = _client.GetStream();
            _listenThread = new Thread(new ThreadStart(Listen));
            _listenThread.Start();
            SendStringMessage(1, version);
        }

        ~TerrariaClient()
        {
            _client.Close();
        }

        public void Listen()
        {
            while (Thread.CurrentThread.IsAlive)
            {
                byte[] buf = new byte[2]; // contains length (uint16)
                _ns.Read(buf, 0, buf.Length);
                ushort length = BitConverter.ToUInt16(buf);
                buf = new byte[1]; // contains type (uint8)
                _ns.Read(buf, 0, buf.Length);
                byte type = buf[0];
                switch (type)
                {
                    case 3: // Authentification confirmation
                        buf = new byte[1];
                        _ns.Read(buf, 0, buf.Length);
                        _slot = buf[0];
                        Console.WriteLine("Player slot is now " + _slot);
                        break;

                    default:
                        Console.WriteLine("Unknown message type " + type);
                        break;
                }
            }
        }

        public void SendStringMessage(byte type, string payload)
        {
            var writer = SendMessage((ushort)payload.Length, type);
            writer.Write(payload);
        }

        private BinaryWriter SendMessage(ushort length, byte type)
        {
            BinaryWriter writer = new BinaryWriter(_ns);
            writer.Write(length);
            writer.Write(type);
            return writer;
        }

        private byte _slot;

        private TcpClient _client;
        private NetworkStream _ns;
        private Thread _listenThread;

        private const string version = "Terraria194";
    }
}
