[![NuGet](https://img.shields.io/nuget/v/TerrariaBot.svg)](https://www.nuget.org/packages/TerrariaBot/)

# TerrariaBot
A library to make Terraria bots in C#

This is currently a work in progress so a lot of things will change in the next days.

For now the bot can barely spawn on the map and do some simple stuff like changing team and enabling PVP.<br/>
Since lot of things are handled client side, that means that the bot will have some impossible behaviour for now, such as not being able to be killed to changing color depending of the client.

## Install it
The project is already on [NuGet](https://www.nuget.org/packages/TerrariaBot/0.2.0-alpha) as a pre-release.
```
Install-Package TerrariaBot -Version 0.2.0-alpha
```
If you want to connect your bot using Steam you'll need TerrariaBot.Steam
```
Coming soon
```
You'll also need to have steam_api64.dll next to your executable

## Example
```cs
private static TerrariaClient client;
static void Main(string[] _)
{
    try
    {
        client = new TerrariaClient(); // We create a new Terraria Client
        client.ServerJoined += Ai; // Function that will be called once the bot is connected
        client.Connect("localhost", PlayerInformation.GetRandomPlayer("MyName", PlayerDifficulty.Easy));
    }
    catch (SocketException se)
    { } // The bot wasn't able to connect to the server
    while (true) // We just wait indefinitly
    { }
}

private static void Ai()
{
    // Just do what you want here, you probably want to interract with your TerrariaClient to do stuffs
}
```

## Projects using this library
Meina is a bot that have for goal to be able to play at Terraria<br/>
 - [GitHub Page](https://github.com/Xwilarg/Meina)
