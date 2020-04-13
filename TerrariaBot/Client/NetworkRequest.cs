﻿using System.ComponentModel;

namespace TerrariaBot.Client
{
    internal enum NetworkRequest
    {
        Authentification = 1,
        FatalError = 2,
        AuthentificationSuccess = 3,
        CharacterCreation = 4,
        CharacterInventorySlot = 5,
        WorldInfoRequest = 6,
        WorldInfoAnswer = 7,
        InitialTileRequest = 8,
        Status = 9,
        TileRowData = 10,
        RecalculateUV = 11,
        SpawnAnswer = 12,
        CharacterHealth = 16,
        BlockUpdate = 20,
        ItemInfo = 21,
        ItemOwnerInfo = 22,
        NPCInfo = 23,
        UpdateProjectile = 27,
        DeleteProjectile = 29,
        TogglePVP = 30,
        PasswordRequest = 37,
        PasswordAnswer = 38,
        CharacterMana = 42,
        JoinTeam = 45,
        SpawnRequest = 49,
        CharacterBuff = 50,
        EvilRatio = 57,
        DailyAnglerQuestFinished = 74,
        EightyTwo = 82,
        EightyThree = 83,
        CharacterStealth = 84,
        InventoryItemInfo = 89,
        NinetySix = 96,
        TowerShieldStrength = 101
    }
}
