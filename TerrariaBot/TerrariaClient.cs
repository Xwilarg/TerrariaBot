using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace TerrariaBot
{
    public class TerrariaClient
    {
        public TerrariaClient(string ip, PlayerInformation playerInfos)
        {
            _slot = 0;
            _playerInfos = playerInfos;

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
                // Length contains the length of the length (2 octets), the type (1 octet) and the payload
                // We remove 3 to only keep the length of the payload
                int length = BitConverter.ToUInt16(buf) - 3;
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
                        SendPlayerInfoMessage();
                        break;

                    default:
                        buf = new byte[length];
                        _ns.Read(buf, 0, buf.Length);
                        Console.WriteLine("Unknown message type " + type);
                        break;
                }
            }
        }

        public void SendPlayerInfoMessage()
        {
            ushort length = (ushort)(29 + _playerInfos.name.Length + 1);
            var writer = SendMessage(length, 4);
            writer.Write(_slot);
            writer.Write((byte)1); // Unknown
            writer.Write(_playerInfos.hairVariant); // Hair variant
            writer.Write(_playerInfos.name); // Name
            writer.Write((byte)1); // Unknown
            writer.Write((byte)1); // Unknown
            writer.Write((byte)1); // Unknown
            writer.Write((byte)1); // Unknown
            writer.WriteColor(_playerInfos.hairColor); // Hair color
            writer.WriteColor(_playerInfos.skinColor); // Skin color
            writer.WriteColor(_playerInfos.eyesColor); // Eyes color
            writer.WriteColor(_playerInfos.shirtColor); // Shirt color
            writer.WriteColor(_playerInfos.underShirtColor); // Under shirt color
            writer.WriteColor(_playerInfos.pantsColor); // Pants color
            writer.WriteColor(_playerInfos.shoesColor); // Shoes color
            writer.Write((byte)_playerInfos.difficulty); // Difficulty
            writer.Flush();
        }

        public void SendStringMessage(byte type, string payload)
        {
            var writer = SendMessage((ushort)(payload.Length + 1), type);
            writer.Write(payload);
            writer.Flush();
        }

        private BinaryWriter SendMessage(ushort length, byte type)
        {
            BinaryWriter writer = new BinaryWriter(_ns);
            writer.Write((ushort)(length + 3));
            writer.Write(type);
            return writer;
        }

        private byte _slot;
        private PlayerInformation _playerInfos;

        private TcpClient _client;
        private NetworkStream _ns;
        private Thread _listenThread;


        private const string version = "Terraria194";
    }
}
