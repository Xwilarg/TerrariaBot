using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using TerrariaBot.Entity;

namespace TerrariaBot.Client
{
    public abstract class AClient
    {
        public AClient()
        {
            _me = null;
            _otherPlayers = new Dictionary<byte, Player>();
            _didSpawn = false;
            _cheats = false;
            _name = null;

            _ms = null;

            _listenThread = new Thread(new ThreadStart(Listen));
        }

        protected abstract byte[] ReadMessage();
        protected abstract void SendMessage(byte[] message);

        /// <summary>
        /// Called when the bot has joined the server
        /// The current player is given in parameter
        /// </summary>
        public event Action<PlayerSelf> ServerJoined;

        /// <summary>
        /// Called when a new player join the server
        /// Also called for each player that are already in the server when you join
        /// The player is given in parameter
        /// </summary>
        public event Action<Player> NewPlayerJoined;

        /// <summary>
        /// Log message of something happening inside the library
        /// The log level and message are given in parameter
        /// </summary>
        public event Action<LogLevel, string> Log;

        /// <summary>
        /// Called when a chat message is sent
        /// Contains the player that sent it along with the message
        /// </summary>
        public event Action<Player, string> ChatMessageReceived;

        /// <summary>
        /// Called when someone changed his PVP status
        /// Contains the player that changed it along with his new status
        /// </summary>
        public event Action<Player, bool> PVPStatusChanged;

        /// <summary>
        /// Called when someone changed his team
        /// Contains the player that changed it along with his new team
        /// </summary>
        public event Action<Player, Team> TeamStatusChanged;

        protected void InitPlayerInfos(PlayerInformation playerInfos, string serverPassword = "")
        {
            _name = playerInfos.GetName();
            _playerInfos = playerInfos;
            _password = serverPassword;
            _listenThread.Start();
            SendStringMessage(NetworkRequest.Authentification, version);
        }

        /// <summary>
        /// Toggle cheats, allow to call functions that would do things normally impossible for a human player
        /// </summary>
        /// <param name="value">True to enable cheats, false to disable them</param>
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
                NetworkRequest type;
                byte[] message = ReadMessage();
                if (message.Length == 0)
                    continue;
                type = (NetworkRequest)message[0];
                var payload = new MemoryStream(message.Skip(1).ToArray());
                BinaryReader reader = new BinaryReader(payload);
                switch (type)
                {
                    case NetworkRequest.FatalError: // Any fatal error that occured lead here
                        reader.ReadByte();
                        throw new System.Exception("Fatal error: " + reader.ReadString());

                    case NetworkRequest.AuthentificationSuccess: // Authentification confirmation
                        {
                            byte slot = reader.ReadByte();
                            _me = new PlayerSelf(this, slot);
                            _otherPlayers.Add(slot, _me);
                            LogDebug("Player slot is now " + slot);
                            SendPlayerInfoMessage();
                            // We don't send our health/mana/etc because we keep the default one
                            if (_playerInfos.GetHealth() != 100)
                                SendPlayerHealth(_playerInfos.GetHealth());
                            if (_playerInfos.GetMana() != 20)
                                SendPlayerMana(_playerInfos.GetMana());
                            SendWorldInfoRequest();
                        }
                        break;

                    case NetworkRequest.CharacterCreation:
                        {
                            byte slot = reader.ReadByte();
                            if (!_otherPlayers.ContainsKey(slot))
                            {
                                LogInfo("New player with slot " + slot);
                                Player player = new Player(this, slot);
                                _otherPlayers.Add(slot, player);
                                NewPlayerJoined?.Invoke(player);
                            }
                        }
                        break;

                    case NetworkRequest.WorldInfoAnswer: // Various basic information about the world
                        if (!_didSpawn)
                        {
                            _didSpawn = true;
                            int time = reader.ReadInt32();
                            byte moonInfo = reader.ReadByte();
                            byte moonPhase = reader.ReadByte();
                            short maxTilesX = reader.ReadInt16();
                            short maxTilesY = reader.ReadInt16();
                            LogDebug("Current time is " + time);
                            LogDebug(ByteToBool(moonInfo, 1) ? "It's currently day time" : "It's currently night time");
                            LogDebug(ByteToBool(moonInfo, 2) ? "It's currently the blood moon" : "It's not currently the blood moon");
                            LogDebug(ByteToBool(moonInfo, 4) ? "It's currently an eclipse" : "It's not currently an eclipse");
                            LogDebug("The current moon phrase is " + moonPhase);
                            LogDebug("Maximum world value at (" + maxTilesX + ";" + maxTilesY + ")");
                            SendInitialTile();
                        }
                        break;

                    case NetworkRequest.TileRowData: // Some information about a row of tile?
                        short width = reader.ReadInt16();
                        int tileX = reader.ReadInt32();
                        int tileY = reader.ReadInt32();
                        LogDebug("Updating " + width + " tiles beginning at (" + tileX + ";" + tileY + ")");
                        break;

                    case NetworkRequest.PlayerControls:
                        {
                            byte slot = reader.ReadByte();
                            byte movement = reader.ReadByte();
                            bool up = ByteToBool(movement, 1);
                            bool down = ByteToBool(movement, 2);
                            bool left = ByteToBool(movement, 4);
                            bool right = ByteToBool(movement, 8);
                            bool jump = ByteToBool(movement, 16);
                            bool useItem = ByteToBool(movement, 32);
                            bool direction = ByteToBool(movement, 64);
                            string keyInfo = "Key pressed: " + (up ? "Up " : "") + (down ? "Down " : "") + (left + "Left " + "") + (right + "Right " + "") + (jump + "Jump " + "");
                            byte otherMovement = reader.ReadByte();
                            byte selectedItem = reader.ReadByte();
                            float posX = reader.ReadSingle();
                            float posY = reader.ReadSingle();
                            if (ByteToBool(otherMovement, 4))
                            {
                                float velX = reader.ReadSingle();
                                float velY = reader.ReadSingle();
                                LogDebug("Player " + slot + " is at (" + posX + ";" + posY + ") with a velocity of (" + velX + ";" + velY + ") " + keyInfo);
                            }
                            LogDebug("Player " + slot + " is at (" + posX + ";" + posY + ") " + keyInfo);
                        }
                        break;

                    case NetworkRequest.SpawnRequest: // When this is received, need to reply with spawn location
                        LogInfo("Sending spawn request at (" + -1 + ";" + -1 + ")");
                        SendSpawnRequest();
                        ServerJoined?.Invoke(_me);
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

                    case NetworkRequest.ChatMessage:
                        {
                            ushort id = reader.ReadUInt16();
                            byte slot = reader.ReadByte();
                            byte mode = reader.ReadByte();
                            try
                            {
                                string content = reader.ReadString();
                                if (mode == 0 && _otherPlayers.ContainsKey(slot))
                                {
                                    ChatMessageReceived?.Invoke(_otherPlayers[slot], content);
                                }
                                else if (mode == 2 && _otherPlayers.ContainsKey(slot) && _messageInfos.ContainsKey(content))
                                {
                                    _messageInfos[content](this, _otherPlayers[slot]);
                                }
                                else
                                    LogDebug("Message received from player " + slot + " with id " + id + " and mode " + mode + ": " + content);
                            }
                            catch (EndOfStreamException) // TODO: Need to fix this
                            {
                                LogDebug("Message received from player " + slot + " with id " + id + " and mode " + mode);
                            }
                        }
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
                    case NetworkRequest.EightyThree:
                    case NetworkRequest.CharacterStealth:
                    case NetworkRequest.InventoryItemInfo:
                    case NetworkRequest.NinetySix:
                    case NetworkRequest.TowerShieldStrength:
                        break;

                    default:
                        // LogDebug("Unknown message type " + type);
                        break;
                }
            }
        }

        private bool ByteToBool(byte b, int offset)
            => (b & offset) != 0;

        internal static void CheatCheck()
        {
            if (!_cheats)
                throw new Exception.CheatNotEnabled();
        }

        #region LogFunctions
        protected void LogDebug<T>(T message)
        {
            Log?.Invoke(LogLevel.Debug, message.ToString());
        }

        protected void LogInfo<T>(T message)
        {
            Log?.Invoke(LogLevel.Info, message.ToString());
        }

        protected void LogWarning<T>(T message)
        {
            Log?.Invoke(LogLevel.Warning, message.ToString());
        }

        protected void LogError<T>(T message)
        {
            Log?.Invoke(LogLevel.Error, message.ToString());
        }
        #endregion LogFunctions

        #region ServerRequestFunctions
        private void SendWorldInfoRequest()
        {
            ushort length = 0;
            WriteHeader(length, NetworkRequest.WorldInfoRequest);
            SendWrittenBytes();
        }

        private void SendPlayerHealth(short health)
        {
            ushort length = 5;
            var writer = WriteHeader(length, NetworkRequest.CharacterHealth);
            writer.Write(_me.GetSlot());
            writer.Write(health);
            writer.Write(health);
            SendWrittenBytes();
        }

        private void SendPlayerMana(short mana)
        {
            ushort length = 5;
            var writer = WriteHeader(length, NetworkRequest.CharacterMana);
            writer.Write(_me.GetSlot());
            writer.Write(mana);
            writer.Write(mana);
            SendWrittenBytes();
        }

        private void SendPlayerBuff()
        {
            ushort length = 11;
            var writer = WriteHeader(length, NetworkRequest.CharacterBuff);
            writer.Write(_me.GetSlot());
            for (int i = 0; i < 10; i++)
                writer.Write((byte)0);
            SendWrittenBytes();
        }

        private void SendPlayerInventorySlot(byte inventorySlot)
        {
            ushort length = 7;
            var writer = WriteHeader(length, NetworkRequest.CharacterMana);
            writer.Write(_me.GetSlot());
            writer.Write(inventorySlot);
            writer.Write((short)0);
            writer.Write((byte)0);
            writer.Write((short)0);
            SendWrittenBytes();
        }

        private void SendInitialTile()
        {
            ushort length = 9;
            var writer = WriteHeader(length, NetworkRequest.InitialTileRequest);
            writer.Write(_me.GetSlot());
            writer.Write(-1);
            writer.Write(-1);
            SendWrittenBytes();
        }

        private void SendSpawnRequest()
        {
            ushort length = 9;
            var writer = WriteHeader(length, NetworkRequest.SpawnAnswer);
            writer.Write(_me.GetSlot());
            writer.Write(-1);
            writer.Write(-1);
            SendWrittenBytes();
        }

        private void SendPlayerInfoMessage()
        {
            ushort length = (ushort)(29 + _playerInfos.GetNameLength() + 1);
            var writer = WriteHeader(length, NetworkRequest.CharacterCreation);
            writer.Write(_me.GetSlot());
            _playerInfos.Write(writer);
            SendWrittenBytes();
        }

        private void SendStringMessage(NetworkRequest type, string payload)
        {
            var writer = WriteHeader((ushort)(payload.Length + 1), type);
            writer.Write(payload);
            SendWrittenBytes();
        }

        internal BinaryWriter WriteHeader(ushort length, NetworkRequest type)
        {
            if (_ms != null)
                _ms.Close();
            _ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(_ms);
            writer.Write((ushort)(length + 3));
            writer.Write((byte)type);
            return writer;
        }

        internal void SendWrittenBytes() => SendMessage(_ms.ToArray());
        #endregion ServerRequestFunctions

        private PlayerSelf _me;
        Dictionary<byte, Player> _otherPlayers;
        private PlayerInformation _playerInfos; // All basic information about the player appearance
        private string _password; // Server password, "" if none
        private bool _didSpawn; // Did the player already spawned
        private static bool _cheats; // Are cheats enabled
        private string _name; internal string GetName() => _name;

        private MemoryStream _ms;

        private Thread _listenThread;

        private const string version = "Terraria194";

        private readonly Dictionary<string, Action<AClient, Player>> _messageInfos = new Dictionary<string, Action<AClient, Player>>()
        {
            { "LegacyMultiplayer.11", (AClient client, Player p) => { client.PVPStatusChanged?.Invoke(p, true); } },
            { "LegacyMultiplayer.12", (AClient client, Player p) => { client.PVPStatusChanged?.Invoke(p, false); } },
            { "LegacyMultiplayer.13", (AClient client, Player p) => { client.TeamStatusChanged?.Invoke(p, Team.None); } },
            { "LegacyMultiplayer.14", (AClient client, Player p) => { client.TeamStatusChanged?.Invoke(p, Team.Red); } },
            { "LegacyMultiplayer.15", (AClient client, Player p) => { client.TeamStatusChanged?.Invoke(p, Team.Green); } },
            { "LegacyMultiplayer.16", (AClient client, Player p) => { client.TeamStatusChanged?.Invoke(p, Team.Blue); } },
            { "LegacyMultiplayer.17", (AClient client, Player p) => { client.TeamStatusChanged?.Invoke(p, Team.Yellow); } },
            { "LegacyMultiplayer.22", (AClient client, Player p) => { client.TeamStatusChanged?.Invoke(p, Team.Pink); } }
        };
    }
}
