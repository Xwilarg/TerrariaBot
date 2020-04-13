using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TerrariaBot.Entity;

namespace TerrariaBot
{
    public class TerrariaClient
    {
        public TerrariaClient(LogLevel logLevel = LogLevel.Info)
        {
            _logLevel = logLevel;

            _me = null;
            _didSpawn = false;
            _cheats = false;

            _spawnX = 0;
            _spawnY = 0;

            _client = null;
            _ns = null;
            _listenThread = new Thread(new ThreadStart(Listen));
        }

        ~TerrariaClient()
        {
            _ns.Close();
            _client.Close();
        }

        public event Action<PlayerSelf> ServerJoined;

        public void Connect(string ip, PlayerInformation playerInfos, string serverPassword = "")
        {
            _playerInfos = playerInfos;
            _password = serverPassword;
            _client = new TcpClient(ip, 7777);
            _ns = _client.GetStream();
            _listenThread.Start();
            SendStringMessage(NetworkRequest.Authentification, version);
        }

        public void ToogleCheats(bool value)
        {
            _cheats = value;
            if (_cheats)
                LogWarning("Cheats were enabled.");
            else
                LogWarning("Cheats were disabled.");
        }

        private void Listen()
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
                    case NetworkRequest.FatalError: // Any fatal error that occured lead here
                        buf = new byte[buf.Length];
                        _ns.Read(buf, 0, buf.Length);
                        throw new System.Exception("Fatal error: " + Encoding.Default.GetString(buf)); // TODO: Doesn't work

                    case NetworkRequest.AuthentificationSuccess: // Authentification confirmation
                        buf = new byte[1];
                        _ns.Read(buf, 0, buf.Length);
                        byte slot = buf[0];
                        _me = new PlayerSelf(this, slot);
                        LogDebug("Player slot is now " + slot);
                        SendPlayerInfoMessage();
                        SendPlayerHealth();
                        SendPlayerMana();
                        SendPlayerBuff();
                        for (byte i = 0; i < 92; i++)
                            SendPlayerInventorySlot(i);
                        SendWorldInfoRequest();
                        break;

                    case NetworkRequest.WorldInfoAnswer: // Various basic information about the world
                        buf = new byte[length];
                        _ns.Read(buf, 0, buf.Length);
                        if (!_didSpawn)
                        {
                            _didSpawn = true;
                            int time = BitConverter.ToInt32(new[] { buf[0], buf[1], buf[2], buf[3] });
                            byte moonInfo = buf[4];
                            byte moonPhase = buf[5];
                            short maxTilesX = BitConverter.ToInt16(new[] { buf[6], buf[7] });
                            short maxTilesY = BitConverter.ToInt16(new[] { buf[8], buf[9] });
                            _spawnX = BitConverter.ToInt16(new[] { buf[10], buf[11] });
                            _spawnY = BitConverter.ToInt16(new[] { buf[12], buf[13] });
                            LogDebug("Current time is " + time);
                            LogDebug(ByteToBool(moonInfo, 1) ? "It's currently day time" : "It's currently night time");
                            LogDebug(ByteToBool(moonInfo, 2) ? "It's currently the blood moon" : "It's not currently the blood moon");
                            LogDebug(ByteToBool(moonInfo, 4) ? "It's currently an eclipse" : "It's not currently an eclipse");
                            LogDebug("The current moon phrase is " + moonPhase);
                            LogDebug("Maximum world value at (" + maxTilesX + ";" + maxTilesY + ")");
                            SendInitialTile(_spawnX, _spawnY);
                        }
                        break;

                    case NetworkRequest.SpawnRequest: // When this is received, need to reply with spawn location
                        LogInfo("Sending spawn request at (" + _spawnX + ";" + _spawnY + ")");
                        SendSpawnRequest();
                        ServerJoined.Invoke(_me);
                        break;

                    case NetworkRequest.PasswordRequest: // The server need a password to be joined
                        if (_password == "")
                            throw new ArgumentException("A password is needed to connect to the server.");
                        else
                        {
                            LogDebug("Sending password to server");
                            SendStringMessage(NetworkRequest.PasswordAnswer, _password);
                        }
                        break;

                    case NetworkRequest.TileRowData: // Some information about a row of tile?
                        buf = new byte[length];
                        _ns.Read(buf, 0, buf.Length);
                        short width = BitConverter.ToInt16(new[] { buf[0], buf[1] });
                        int tileX = BitConverter.ToInt32(new[] { buf[2], buf[3], buf[4], buf[5] });
                        int tileY = BitConverter.ToInt32(new[] { buf[6], buf[7], buf[8], buf[9] });
                        LogDebug("Updating " + width + " tiles beginning at (" + tileX + ";" + tileY + ")");
                        break;

                    case NetworkRequest.CharacterInventorySlot:
                    case NetworkRequest.Status:
                    case NetworkRequest.RecalculateUV:
                    case NetworkRequest.BlockUpdate:
                    case NetworkRequest.ItemInfo:
                    case NetworkRequest.ItemOwnerInfo:
                    case NetworkRequest.NPCInfo:
                    case NetworkRequest.UpdateProjectile:
                    case NetworkRequest.DeleteProjectile:
                    case NetworkRequest.EvilRatio:
                    case NetworkRequest.DailyAnglerQuestFinished:
                    case NetworkRequest.EightyTwo:
                    case NetworkRequest.EightyThree:
                    case NetworkRequest.CharacterStealth:
                    case NetworkRequest.InventoryItemInfo:
                    case NetworkRequest.NinetySix:
                    case NetworkRequest.TowerShieldStrength:
                        buf = new byte[length];
                        _ns.Read(buf, 0, buf.Length);
                        break;

                    default:
                        if (length > 0)
                        {
                            buf = new byte[length];
                            _ns.Read(buf, 0, buf.Length);
                        }
                        LogDebug("Unknown message type " + type);
                        break;
                }
            }
        }

        private bool ByteToBool(byte b, int offset)
            => (b & offset) != 0;

        private void CheatCheck()
        {
            if (!_cheats)
                throw new Exception.CheatNotEnabled();
        }

        #region LogFunctions
        private void LogDebug<T>(T message)
        {
            if (_logLevel == LogLevel.Debug)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(message);
                Console.ForegroundColor = color;
            }
        }

        private void LogInfo<T>(T message)
        {
            if (_logLevel <= LogLevel.Info)
                Console.WriteLine(message);
        }

        private void LogWarning<T>(T message)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = color;
        }
        #endregion LogFunctions

        #region ServerRequestFunctions
        private void SendWorldInfoRequest()
        {
            ushort length = 0;
            var writer = SendMessage(length, NetworkRequest.WorldInfoRequest);
            writer.Flush();
        }

        private void SendPlayerHealth()
        {
            ushort length = 5;
            var writer = SendMessage(length, NetworkRequest.CharacterHealth);
            writer.Write(_me.GetSlot());
            writer.Write((short)100);
            writer.Write((short)100);
            writer.Flush();
        }

        private void SendPlayerMana()
        {
            ushort length = 5;
            var writer = SendMessage(length, NetworkRequest.CharacterMana);
            writer.Write(_me.GetSlot());
            writer.Write((short)20);
            writer.Write((short)20);
            writer.Flush();
        }

        private void SendPlayerBuff()
        {
            ushort length = 11;
            var writer = SendMessage(length, NetworkRequest.CharacterBuff);
            writer.Write(_me.GetSlot());
            for (int i = 0; i < 10; i++)
                writer.Write((byte)0);
            writer.Flush();
        }

        private void SendPlayerInventorySlot(byte inventorySlot)
        {
            ushort length = 7;
            var writer = SendMessage(length, NetworkRequest.CharacterMana);
            writer.Write(_me.GetSlot());
            writer.Write(inventorySlot);
            writer.Write((short)0);
            writer.Write((byte)0);
            writer.Write((short)0);
            writer.Flush();
        }

        private void SendInitialTile(int spawnX, int spawnY)
        {
            ushort length = 9;
            var writer = SendMessage(length, NetworkRequest.InitialTileRequest);
            writer.Write(_me.GetSlot());
            writer.Write(spawnX);
            writer.Write(spawnY);
            writer.Flush();
        }

        private void SendSpawnRequest()
        {
            ushort length = 9;
            var writer = SendMessage(length, NetworkRequest.SpawnAnswer);
            writer.Write(_me.GetSlot());
            writer.Write(_spawnX);
            writer.Write(_spawnY);
            writer.Flush();
        }

        private void SendPlayerInfoMessage()
        {
            ushort length = (ushort)(29 + _playerInfos.name.Length + 1);
            var writer = SendMessage(length, NetworkRequest.CharacterCreation);
            writer.Write(_me.GetSlot());
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

        private void SendStringMessage(NetworkRequest type, string payload)
        {
            if (_client == null)
                throw new NullReferenceException("You must call Connect() before doing bot requests");
            var writer = SendMessage((ushort)(payload.Length + 1), type);
            writer.Write(payload);
            writer.Flush();
        }

        internal BinaryWriter SendMessage(ushort length, NetworkRequest type)
        {
            BinaryWriter writer = new BinaryWriter(_ns);
            writer.Write((ushort)(length + 3));
            writer.Write((byte)type);
            return writer;
        }
        #endregion ServerRequestFunctions

        private readonly LogLevel _logLevel;

        private PlayerSelf _me;
        private PlayerInformation _playerInfos; // All basic information about the player appearance
        private string _password; // Server password, "" if none
        private bool _didSpawn; // Did the player already spawned
        private bool _cheats; // Are cheats enabled

        private int _spawnX, _spawnY; // Spawn position

        private TcpClient _client;
        private NetworkStream _ns;
        private Thread _listenThread;

        private const string version = "Terraria194";
    }
}
