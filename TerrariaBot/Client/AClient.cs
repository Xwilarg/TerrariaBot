using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
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
            _tiles = null;
            _tileFrameImportant = new bool[470];
            foreach (var frame in _importantFrames)
            {
                _tileFrameImportant[frame] = true;
            }
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

        /// <summary>
        /// Called when a player change his position
        /// Contains the player that moved along with his new position
        /// </summary>
        public event Action<Player, Vector2> PlayerPositionUpdate;

        /// <summary>
        /// Get the current player
        /// </summary>
        public PlayerSelf GetPlayerSelf() => _me;

        /// <summary>
        /// Get all the players (including the current player)
        /// </summary>
        public Player[] GetAllPlayers() => _otherPlayers.Values.ToArray();

        /// <summary>
        /// Get a player given it id
        /// </summary>
        public Player GetPlayer(byte id) => _otherPlayers[id];

        protected void InitPlayerInfos(PlayerInformation playerInfos, string serverPassword = "")
        {
            _name = playerInfos.GetName();
            _playerInfos = playerInfos;
            _password = serverPassword;
            _listenThread.Start();
            SendStringMessage(NetworkRequest.Authentification, version);
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
                        throw new Exception("Fatal error: " + reader.ReadString());

                    case NetworkRequest.AuthentificationSuccess: // Authentification confirmation
                        {
                            byte slot = reader.ReadByte();
                            _me = new PlayerSelf(this, _playerInfos.GetName(), slot);
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
                            reader.ReadByte();
                            reader.ReadByte();
                            string name = reader.ReadString();
                            if (!_otherPlayers.ContainsKey(slot))
                            {
                                LogInfo("New player with slot " + slot);
                                Player player = new Player(this, name, slot);
                                _otherPlayers.Add(slot, player);
                                NewPlayerJoined?.Invoke(player);
                            }
                        }
                        break;

                    case NetworkRequest.WorldInfoAnswer: // Various basic information about the world
                        if (!_didSpawn)
                        {
                            _didSpawn = true;
                            var blocPixelSize = BlocGroup.blocPixelSize;
                            int time = reader.ReadInt32();
                            byte moonInfo = reader.ReadByte();
                            byte moonPhase = reader.ReadByte();
                            short maxTilesX = reader.ReadInt16();
                            short maxTilesY = reader.ReadInt16();
                            short spawnX = reader.ReadInt16();
                            short spawnY = reader.ReadInt16();
                            short surfaceY = reader.ReadInt16();
                            short rockY = reader.ReadInt16();
                            LogDebug("Current time is " + time);
                            LogDebug(ByteToBool(moonInfo, 1) ? "It's currently day time" : "It's currently night time");
                            LogDebug(ByteToBool(moonInfo, 2) ? "It's currently the blood moon" : "It's not currently the blood moon");
                            LogDebug(ByteToBool(moonInfo, 4) ? "It's currently an eclipse" : "It's not currently an eclipse");
                            LogDebug("The current moon phrase is " + moonPhase);
                            LogDebug("Maximum world value at (" + (maxTilesX * blocPixelSize) + ";" + (maxTilesY * blocPixelSize) + ")");
                            LogDebug("Spawn world value at (" + (spawnX * blocPixelSize) + ";" + (spawnY * blocPixelSize) + ")");
                            LogDebug("Surface layer is at heigth of " + surfaceY);
                            LogDebug("Rock layer is at height of " + rockY);
                            _me.SetPosition(new Vector2(spawnX * blocPixelSize, spawnY * blocPixelSize), Vector2.Zero);
                            _tiles = new Tile[maxTilesX, maxTilesY];
                            for (int i = 0; i < maxTilesX; i++)
                                for (int y = 0; y < maxTilesY; y++)
                                    _tiles[i, y] = null;
                            SendInitialTile();
                        }
                        break;

                    case NetworkRequest.TileData: // Some information about a row of tile?
                        {
                            MemoryStream stream = new MemoryStream();
                            if (payload.ReadByte() != 0)
                            {
                                using (DeflateStream deflate = new DeflateStream(payload, CompressionMode.Decompress))
                                {
                                    deflate.CopyTo(stream);
                                    deflate.Close();
                                }
                                stream.Position = 0L;
                            }
                            else
                            {
                                stream = payload;
                                stream.Position = 1L;
                            }
                            using (BinaryReader r = new BinaryReader(stream))
                            {
                                int xStart = r.ReadInt32();
                                int yStart = r.ReadInt32();
                                int width = r.ReadInt16();
                                int height = r.ReadInt16();
                                LogDebug("Updating " + width + " x " + height + " tiles beginning at (" + xStart + ";" + yStart + ")");
                                // I have no idea what I'm doing but it's what Terraria is doing
                                Tile tile = null;
                                int value = 0;
                                for (int y = yStart; y < yStart + height; y++)
                                {
                                    for (int x = xStart; x < xStart + width; x++)
                                    {
                                        if (value != 0)
                                        {
                                            value--;
                                            _tiles[x, y] = (tile == null ? new Tile() : new Tile(tile));
                                        }
                                        else
                                        {
                                            byte b = 0;
                                            byte b2 = 0;
                                            tile = _tiles[x, y];
                                            _tiles[x, y] = new Tile();
                                            if (tile == null)
                                                tile = _tiles[x, y];
                                            byte b3 = r.ReadByte();
                                            if ((b3 & 1) == 1)
                                            {
                                                b2 = r.ReadByte();
                                                if ((b2 & 1) == 1)
                                                {
                                                    b = r.ReadByte();
                                                }
                                            }
                                            bool flag = tile.IsActive();
                                            byte b4;
                                            if ((b3 & 2) == 2)
                                            {
                                                tile.Activate(true);
                                                ushort ttype = tile.GetTileType();
                                                int num2;
                                                if ((b3 & 32) == 32)
                                                {
                                                    b4 = r.ReadByte();
                                                    num2 = r.ReadByte();
                                                    num2 = (num2 << 8 | b4);
                                                }
                                                else
                                                {
                                                    num2 = r.ReadByte();
                                                }
                                                tile.SetTileType((ushort)num2);
                                                // num2 < _tileFrameImportant.Length shouldn't be here
                                                if (num2 > _tileFrameImportant.Length)
                                                {
                                                    LogError(num2 + " is bigger than _tileFrameImportant length (" + _tileFrameImportant.Length + ")");
                                                }
                                                if (num2 < _tileFrameImportant.Length && _tileFrameImportant[num2])
                                                {
                                                    tile.SetFrames(r.ReadInt16(), r.ReadInt16());
                                                }
                                                else if (!flag || tile.GetTileType() != ttype)
                                                {
                                                    tile.SetFrames(-1, -1);
                                                }
                                                if ((b & 8) == 8)
                                                {
                                                    tile.Color(r.ReadByte());
                                                }
                                            }
                                            if ((b3 & 4) == 4)
                                            {
                                                tile.SetWall(r.ReadByte());
                                                if ((b & 16) == 16)
                                                {
                                                    tile.ColorWall(r.ReadByte());
                                                }
                                            }
                                            b4 = (byte)((b3 & 24) >> 3);
                                            if (b4 != 0)
                                            {
                                                tile.SetLiquid(r.ReadByte());
                                                if (b4 > 1)
                                                {
                                                    // Lava and honey management
                                                }
                                            }
                                            if (b2 > 1)
                                            {
                                                // Wires and slops management
                                            }
                                            if (b > 0)
                                            {
                                                // Some others electrical management
                                            }
                                            b4 = (byte)((b3 & 192) >> 6);
                                            if (b4 == 0)
                                            {
                                                value = 0;
                                            }
                                            else if (b4 == 1)
                                            {
                                                value = r.ReadByte();
                                            }
                                            else
                                            {
                                                value = r.ReadInt16();
                                            }
                                        }
                                    }
                                }
                            }
                        }
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
                            string keyInfo = "Key pressed: " + (up ? "Up " : "") + (down ? ", Down " : "") + (left + ", Left " + "") + (right + ", Right " + "") + (jump + ", Jump " + "");
                            byte otherMovement = reader.ReadByte();
                            byte selectedItem = reader.ReadByte();
                            float posX = reader.ReadSingle();
                            float posY = reader.ReadSingle();
                            var player = _otherPlayers[slot];
                            var newPos = new Vector2(posX, posY);
                            float velX = 0f;
                            float velY = 0f;
                            PlayerPositionUpdate?.Invoke(player, newPos);
                            if (ByteToBool(otherMovement, 4))
                            {
                                velX = reader.ReadSingle();
                                velY = reader.ReadSingle();
                                LogDebug("Player " + slot + " is at (" + posX + ";" + posY + ") with a velocity of (" + velX + ";" + velY + ") " + keyInfo);
                            }
                            player.SetPosition(newPos, new Vector2(velX, velY));
                            LogDebug("Player " + slot + " is at (" + posX + ";" + posY + ") " + keyInfo);
                        }
                        break;

                    case NetworkRequest.TileEdit:
                        {
                            byte action = reader.ReadByte();
                            short xPos = reader.ReadInt16();
                            short yPos = reader.ReadInt16();
                            short tileId = reader.ReadInt16();
                            if (action == 0) // Tile destroyed
                                _tiles[xPos, yPos] = new Tile();
                            else
                                _tiles[xPos, yPos] = new Tile((byte)tileId);
                        }
                        break;

                    case NetworkRequest.TogglePVP:
                        {
                            byte slot = reader.ReadByte();
                            bool isPVP = reader.ReadByte() == 1;
                            PVPStatusChanged?.Invoke(_otherPlayers[slot], isPVP);
                        }
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

                    case NetworkRequest.JoinTeam:
                        {
                            byte slot = reader.ReadByte();
                            byte team = reader.ReadByte();
                            TeamStatusChanged?.Invoke(_otherPlayers[slot], (Team)team);
                        }
                        break;

                    case NetworkRequest.SpawnRequest: // When this is received, need to reply with spawn location
                        LogInfo("Sending spawn request at (" + -1 + ";" + -1 + ")");
                        SendSpawnRequest();
                        ServerJoined?.Invoke(_me);
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
                                else if (mode == 2 && slot == 255)
                                {
                                    LogDebug("Message received from server with id " + id + " and mode " + mode + ": " + content);
                                }
                            }
                            catch (EndOfStreamException) // TODO: Need to fix this
                            { }
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

        internal void SendPlayerControls(Player p, byte actions, float xVel, float yVel)
        {
            var myPos = _me.GetPosition();
            _me.SetPosition(myPos, new Vector2(xVel, yVel));
            ushort length = 20;
            var writter = WriteHeader(length, NetworkRequest.PlayerControls);
            writter.Write(p.GetSlot());
            writter.Write(actions);
            writter.Write((byte)0);
            writter.Write((byte)0);
            writter.Write(myPos.X);
            writter.Write(myPos.Y);
            writter.Write(xVel);
            writter.Write(yVel);
            SendWrittenBytes();
        }

        internal void SendTeleport(Player p, float x, float y)
        {
            _me.SetPosition(new Vector2(x, y), Vector2.Zero);
            ushort length = 20;
            var writter = WriteHeader(length, NetworkRequest.PlayerControls);
            writter.Write(p.GetSlot());
            writter.Write((byte)0);
            writter.Write((byte)0);
            writter.Write((byte)0);
            writter.Write(x);
            writter.Write(y);
            writter.Write(0f);
            writter.Write(0f);
            SendWrittenBytes();
            PlayerPositionUpdate?.Invoke(p, new Vector2(x, y));
        }

        internal void JoinTeam(Player p, Team teamId)
        {
            ushort length = 2;
            var writer = WriteHeader(length, NetworkRequest.JoinTeam);
            writer.Write(p.GetSlot());
            writer.Write((byte)teamId);
            SendWrittenBytes();
            TeamStatusChanged?.Invoke(p, teamId);
        }

        internal void TogglePVP(Player p, bool status)
        {
            ushort length = 2;
            var writer = WriteHeader(length, NetworkRequest.TogglePVP);
            writer.Write(p.GetSlot());
            writer.Write((byte)(status ? 1 : 0));
            SendWrittenBytes();
            PVPStatusChanged?.Invoke(p, status);
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
        private Tile[,] _tiles;
        internal Tile GetTile(int x, int y) => _tiles[x, y];
        private bool[] _tileFrameImportant;
        private readonly int[] _importantFrames = new[]
        {
            377, 373, 375, 374, 461, 372,
            358, 359, 360, 361, 362, 363, 364, 391,
            392, 393, 394, 356, 334, 440, 300, 301, 302, 303, 304, 305, 306, 307, 308, 354, 355, 324,
            463, 464, 466, 419, 442, 443, 444, 420, 423, 424, 428, 429, 445, 283, 288, 289, 290, 291, 292, 293, 294, 295, 296, 297, 316, 317, 318,
            410, 427, 435, 436, 437, 438, 439,
            36, 275, 276, 277, 278, 279, 280, 281, 282, 285, 286, 414, 413, 309, 310, 339,
            289, 299, 171, 247, 245, 246, 239, 240, 241, 242, 243, 244, 254,
            237, 238, 235, 236, 269, 390,
            233, 227, 228, 231,
            216, 217, 218, 219, 200, 338, 453, 456, 165, 209, 215, 210, 212, 207, 178, 184, 185, 186, 187, 173, 174,
            139, 149, 142, 143, 144, 136, 137, 138,
            320,
            380, 201, 3, 4, 5, 10, 11, 12, 13, 14, 469, 15, 16, 17, 18, 19, 20, 21, 467, 441, 468, 24, 26, 27, 28, 29, 31, 33, 34, 35,
            42, 50, 55, 61, 71, 72, 73, 74, 77, 78, 79, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99,
            101, 102, 103, 104, 105, 100, 106, 110, 113, 114, 125, 287, 126, 128, 129, 132, 133, 134, 135, 172, 319, 323, 335, 337,
            349, 376, 378, 425, 465, 141, 270, 271, 314,
            395, 405, 406, 452, 411, 457, 462, 454, 455, 412,
            387, 386, 388, 389
        };
        private string _name; internal string GetName() => _name;

        private MemoryStream _ms;

        private Thread _listenThread;

        private const string version = "Terraria194";
    }
}
