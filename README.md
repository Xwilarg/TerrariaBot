[![NuGet](https://img.shields.io/nuget/v/TerrariaBot.svg)](https://www.nuget.org/packages/TerrariaBot/)

# TerrariaBot
A library to make Terraria bots in C#

This is currently a work in progress so a lot of things will change in the next days.

The documentation is available here: https://terrariabot.zirk.eu

## Install it
The project is already on [NuGet](https://www.nuget.org/packages/TerrariaBot/0.3.0-alpha) as a pre-release.
```
Install-Package TerrariaBot -Version 0.4.0-alpha
```

## Connect to another player using Steam (coming soon)
If you want to connect your bot using Steam you'll need TerrariaBot.Steam<br/>
Please make not that using it will use your Steam account and increase your hours played on Steam
```
Nuget package coming soon
```
The next step will be to add a dependency to [Steamworks.NET](https://github.com/rlabrecque/Steamworks.NET/releases)<br/>
Then you'll need do add steam_api64.dll and Steamworks.NET.dll next to your executable

## Example
```cs
private static IPClient client;
private static AutoResetEvent autoEvent = new AutoResetEvent(false);
static void Main(string[] _)
{
    try
    {
        client = new IPClient(); // We create a new Terraria Client
        client.ServerJoined += Ai; // Function that will be called once the bot is connected
        client.ConnectWithIP("localhost", PlayerInformation.GetRandomPlayer("MyName", PlayerDifficulty.Easy));
    }
    catch (SocketException se)
    { } // The bot wasn't able to connect to the server
    autoEvent.WaitOne(); // We just wait indefinitly
}

private static void Ai(PlayerSelf me)
{
    // Just do what you want here, you probably want to interract with your PlayerSelf to do stuffs
}
```

## Projects using this library
Meina is a bot that have for goal to be able to play at Terraria<br/>
 - [GitHub Page](https://github.com/Xwilarg/Meina)

## So how does it works?
If you want to understand better how Terraria client communicate you can check this repository: https://github.com/TheIndra55/terraria-research

# Need more help?
Feel free to [open an issue](https://github.com/Xwilarg/TerrariaBot/issues) or come ask [on Discord](https://discord.gg/H6wMRYV).
