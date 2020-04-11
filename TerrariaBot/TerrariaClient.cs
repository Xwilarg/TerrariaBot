using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TerrariaBot
{
    public class TerrariaClient
    {
        public TerrariaClient(string ip, PlayerInformation playerInfos, string serverPassword = "")
        {
            _slot = 0;
            _playerInfos = playerInfos;
            _password = serverPassword;
            didSpawn = false;

            _spawnX = 0;
            _spawnY = 0;

            _client = new TcpClient(ip, 7777);
            _ns = _client.GetStream();
            _listenThread = new Thread(new ThreadStart(Listen));
            _listenThread.Start();
            SendStringMessage(NetworkRequest.Authentification, version);
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
                switch ((NetworkRequest)type)
                {
                    case NetworkRequest.FatalError: // Authentification confirmation
                        buf = new byte[buf.Length];
                        _ns.Read(buf, 0, buf.Length);
                        throw new Exception("Fatal error: " + Encoding.Default.GetString(buf)); // TODO: Doesn't work

                    case NetworkRequest.AuthentificationSuccess: // Authentification confirmation
                        buf = new byte[1];
                        _ns.Read(buf, 0, buf.Length);
                        _slot = buf[0];
                        Console.WriteLine("Player slot is now " + _slot);
                        SendPlayerInfoMessage();
                        SendPlayerHealth();
                        SendPlayerMana();
                        SendPlayerBuff();
                        for (byte i = 0; i < 92; i++)
                            SendPlayerInventorySlot(i);
                        SendWorldInfoRequest();
                        break;

                    case NetworkRequest.WorldInfoAnswer:
                        buf = new byte[length];
                        _ns.Read(buf, 0, buf.Length);
                        Console.WriteLine("World info received");
                        if (!didSpawn)
                        {
                            didSpawn = true;
                            _spawnX = BitConverter.ToInt32(new[] { buf[16], buf[17], buf[18], buf[19] });
                            _spawnY = BitConverter.ToInt32(new[] { buf[20], buf[21], buf[22], buf[23] });
                            SendInitialTile(_spawnX, _spawnY);
                        }
                        break;

                    case NetworkRequest.SpawnRequest:
                        Console.WriteLine("Sending spawn request");
                        SendSpawnRequest();
                        break;

                    case NetworkRequest.PasswordRequest:
                        if (_password == "")
                            throw new ArgumentException("A password is needed to connect to the server.");
                        else
                        {
                            Console.WriteLine("Sending password to server");
                            SendStringMessage(NetworkRequest.PasswordAnswer, _password);
                        }
                        break;

                    case NetworkRequest.EightyTwo:
                        buf = new byte[length];
                        _ns.Read(buf, 0, buf.Length);
                        Console.WriteLine("82");
                        break;

                    default:
                        if (length > 0)
                        {
                            buf = new byte[length];
                            _ns.Read(buf, 0, buf.Length);
                        }
                        Console.WriteLine("Unknown message type " + type);
                        break;
                }
            }
        }

        public void SendWorldInfoRequest()
        {
            ushort length = 0;
            var writer = SendMessage(length, NetworkRequest.WorldInfoRequest);
            writer.Flush();
        }

        public void SendPlayerHealth()
        {
            ushort length = 5;
            var writer = SendMessage(length, NetworkRequest.CharacterHealth);
            writer.Write(_slot);
            writer.Write((short)100);
            writer.Write((short)100);
            writer.Flush();
        }

        public void SendPlayerMana()
        {
            ushort length = 5;
            var writer = SendMessage(length, NetworkRequest.CharacterMana);
            writer.Write(_slot);
            writer.Write((short)20);
            writer.Write((short)20);
            writer.Flush();
        }

        public void SendPlayerBuff()
        {
            ushort length = 11;
            var writer = SendMessage(length, NetworkRequest.CharacterBuff);
            writer.Write(_slot);
            for (int i = 0; i < 10; i++)
                writer.Write((byte)0);
            writer.Flush();
        }

        public void SendPlayerInventorySlot(byte inventorySlot)
        {
            ushort length = 7;
            var writer = SendMessage(length, NetworkRequest.CharacterMana);
            writer.Write(_slot);
            writer.Write(inventorySlot);
            writer.Write((short)0);
            writer.Write((byte)0);
            writer.Write((short)0);
            writer.Flush();
        }

        public void SendInitialTile(int spawnX, int spawnY)
        {
            ushort length = 9;
            var writer = SendMessage(length, NetworkRequest.InitialTileRequest);
            writer.Write(_slot);
            writer.Write(spawnX);
            writer.Write(spawnY);
            writer.Flush();
        }

        public void SendSpawnRequest()
        {
            ushort length = 9;
            var writer = SendMessage(length, NetworkRequest.SpawnAnswer);
            writer.Write(_slot);
            writer.Write(_spawnX);
            writer.Write(_spawnY);
            writer.Flush();
        }

        public void SendPlayerInfoMessage()
        {
            ushort length = (ushort)(29 + _playerInfos.name.Length + 1);
            var writer = SendMessage(length, NetworkRequest.CharacterCreation);
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

        public void SendStringMessage(NetworkRequest type, string payload)
        {
            var writer = SendMessage((ushort)(payload.Length + 1), type);
            writer.Write(payload);
            writer.Flush();
        }

        private BinaryWriter SendMessage(ushort length, NetworkRequest type)
        {
            BinaryWriter writer = new BinaryWriter(_ns);
            writer.Write((ushort)(length + 3));
            writer.Write((byte)type);
            return writer;
        }

        private byte _slot;
        private PlayerInformation _playerInfos;
        private readonly string _password;
        private bool didSpawn;

        private int _spawnX, _spawnY;

        private TcpClient _client;
        private NetworkStream _ns;
        private Thread _listenThread;

        private const string version = "Terraria194";
    }
}
