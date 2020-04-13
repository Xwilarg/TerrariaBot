﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using TerrariaBot.Entity;

namespace TerrariaBot.Client
{
    public abstract class AClient
    {
        public AClient(LogLevel logLevel = LogLevel.Info)
        {
            _logLevel = logLevel;

            _me = null;
            _otherPlayers = new Dictionary<byte, Player>();
            _didSpawn = false;
            _cheats = false;

            _ms = null;

            _listenThread = new Thread(new ThreadStart(Listen));
        }

        internal abstract byte[] ReadMessage();
        internal abstract void SendMessage(byte[] message);

        public event Action<PlayerSelf> ServerJoined;

        protected void InitPlayerInfos(PlayerInformation playerInfos, string serverPassword = "", PlayerStartModifier? modifier = null)
        {
            if (modifier != null)
                CheatCheck();
            _playerInfos = playerInfos;
            _modifier = modifier;
            _password = serverPassword;
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
                NetworkRequest type;
                byte[] message = ReadMessage();
                type = (NetworkRequest)message[0];
                byte[] payload;
                payload = message.Skip(1).ToArray();
                switch (type)
                {
                    case NetworkRequest.FatalError: // Any fatal error that occured lead here
                        throw new System.Exception("Fatal error: " + Encoding.Default.GetString(payload)); // TODO: Doesn't work

                    case NetworkRequest.AuthentificationSuccess: // Authentification confirmation
                        {
                            byte slot = payload[0];
                            _me = new PlayerSelf(this, slot);
                            LogDebug("Player slot is now " + slot);
                            SendPlayerInfoMessage();
                            // We don't send our health/mana/etc because we keep the default one
                            if (_modifier != null)
                            {
                                if (_modifier.Value.healthModifier.HasValue)
                                    SendPlayerHealth(_modifier.Value.healthModifier.Value);
                                if (_modifier.Value.manaModifier.HasValue)
                                    SendPlayerMana(_modifier.Value.manaModifier.Value);
                            }
                            SendWorldInfoRequest();
                        }
                        break;

                    case NetworkRequest.CharacterCreation:
                        {
                            byte slot = payload[0];
                            if (!_otherPlayers.ContainsKey(slot))
                            {
                                LogInfo("New player with slot " + slot);
                                _otherPlayers.Add(slot, new Player(this, slot));
                            }
                        }
                        break;

                    case NetworkRequest.WorldInfoAnswer: // Various basic information about the world
                        if (!_didSpawn)
                        {
                            _didSpawn = true;
                            int time = BitConverter.ToInt32(new[] { payload[0], payload[1], payload[2], payload[3] });
                            byte moonInfo = payload[4];
                            byte moonPhase = payload[5];
                            short maxTilesX = BitConverter.ToInt16(new[] { payload[6], payload[7] });
                            short maxTilesY = BitConverter.ToInt16(new[] { payload[8], payload[9] });
                            LogDebug("Current time is " + time);
                            LogDebug(ByteToBool(moonInfo, 1) ? "It's currently day time" : "It's currently night time");
                            LogDebug(ByteToBool(moonInfo, 2) ? "It's currently the blood moon" : "It's not currently the blood moon");
                            LogDebug(ByteToBool(moonInfo, 4) ? "It's currently an eclipse" : "It's not currently an eclipse");
                            LogDebug("The current moon phrase is " + moonPhase);
                            LogDebug("Maximum world value at (" + maxTilesX + ";" + maxTilesY + ")");
                            SendInitialTile();
                        }
                        break;

                    case NetworkRequest.SpawnRequest: // When this is received, need to reply with spawn location
                        LogInfo("Sending spawn request at (" + -1 + ";" + -1 + ")");
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
                        short width = BitConverter.ToInt16(new[] { payload[0], payload[1] });
                        int tileX = BitConverter.ToInt32(new[] { payload[2], payload[3], payload[4], payload[5] });
                        int tileY = BitConverter.ToInt32(new[] { payload[6], payload[7], payload[8], payload[9] });
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
                        break;

                    default:
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
        protected void LogDebug<T>(T message)
        {
            if (_logLevel == LogLevel.Debug)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(message);
                Console.ForegroundColor = color;
            }
        }

        protected void LogInfo<T>(T message)
        {
            if (_logLevel <= LogLevel.Info)
                Console.WriteLine(message);
        }

        protected void LogWarning<T>(T message)
        {
            var color = Console.ForegroundColor;
            if (_logLevel <= LogLevel.Warning)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(message);
                Console.ForegroundColor = color;
            }
        }

        protected void LogError<T>(T message)
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
            ushort length = (ushort)(29 + _playerInfos.name.Length + 1);
            var writer = WriteHeader(length, NetworkRequest.CharacterCreation);
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

        private readonly LogLevel _logLevel;

        private PlayerSelf _me;
        Dictionary<byte, Player> _otherPlayers;
        private PlayerInformation _playerInfos; // All basic information about the player appearance
        private PlayerStartModifier? _modifier;
        private string _password; // Server password, "" if none
        private bool _didSpawn; // Did the player already spawned
        private bool _cheats; // Are cheats enabled

        private MemoryStream _ms;

        private Thread _listenThread;

        private const string version = "Terraria194";
    }
}