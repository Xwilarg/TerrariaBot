namespace TerrariaBot
{
    public enum NetworkRequest
    {
        Authentification = 1,
        FatalError = 2,
        AuthentificationSuccess = 3,
        CharacterCreation = 4,
        CharacterInventorySlot = 5,
        WorldInfoRequest = 6,
        WorldInfoAnswer = 7,
        InitialTileRequest = 8,
        SpawnAnswer = 12,
        CharacterHealth = 16,
        PasswordRequest = 37,
        PasswordAnswer = 38,
        CharacterMana = 42,
        JoinTeam = 45,
        SpawnRequest = 49,
        CharacterBuff = 50,
        EightyTwo = 82
    }
}
