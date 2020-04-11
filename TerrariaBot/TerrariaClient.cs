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
                        Console.WriteLine(length);
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
            ushort length = (ushort)(37 + _playerInfos.name.Length);
            var writer = SendMessage(length, 4);
            writer.Write(_slot);
            writer.Write((ushort)1); // Unknown
            writer.Write(_playerInfos.hairVariant); // Hair variant
            writer.Write(_playerInfos.name); // Name
            writer.Write((ushort)1); // Unknown
            writer.Write((ushort)1); // Unknown
            writer.Write((ushort)1); // Unknown
            writer.Write((ushort)1); // Unknown
            writer.Write(new byte[] { _playerInfos.hairColor.r, _playerInfos.hairColor.g, _playerInfos.hairColor.b }); // Hair color
            writer.Write(new byte[] { _playerInfos.skinColor.r, _playerInfos.skinColor.g, _playerInfos.skinColor.b }); // Skin color
            writer.Write(new byte[] { _playerInfos.eyesColor.r, _playerInfos.eyesColor.g, _playerInfos.eyesColor.b }); // Eyes color
            writer.Write(new byte[] { _playerInfos.shirtColor.r, _playerInfos.shirtColor.g, _playerInfos.shirtColor.b }); // Shirt color
            writer.Write(new byte[] { _playerInfos.underShirtColor.r, _playerInfos.underShirtColor.g, _playerInfos.underShirtColor.b }); // Under shirt color
            writer.Write(new byte[] { _playerInfos.pantsColor.r, _playerInfos.pantsColor.g, _playerInfos.pantsColor.b }); // Pants color
            writer.Write(new byte[] { _playerInfos.shoesColor.r, _playerInfos.shoesColor.g, _playerInfos.shoesColor.b }); // Shoes color
            writer.Write((ushort)_playerInfos.difficulty); // Difficulty
            writer.Flush();
        }

        public void SendStringMessage(byte type, string payload)
        {
            var writer = SendMessage((ushort)payload.Length, type);
            writer.Write(payload);
            writer.Flush();
        }

        private BinaryWriter SendMessage(ushort length, byte type)
        {
            BinaryWriter writer = new BinaryWriter(_ns);
            writer.Write(length);
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
