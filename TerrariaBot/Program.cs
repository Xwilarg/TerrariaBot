using System;
using System.Net.Sockets;

namespace TerrariaBot
{
    class Program
    {
        static void Exit()
        {
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
            Environment.Exit(1);
        }

        static void Main(string[] args)
        {
            TerrariaClient client;
            try
            {
                client = new TerrariaClient("localhost");
            }
            catch (SocketException se)
            {
                Console.WriteLine("Can't connect to server: " + se.Message + Environment.NewLine + "Make sure that your Terraria server is online.");
                Exit();
            }
            Console.WriteLine("Bot is connected");
            while (true)
            { }
        }
    }
}
