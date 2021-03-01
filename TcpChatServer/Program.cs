using System;

namespace TcpChatServer
{
    class Program
    {
        private static Server server;

        static void Main(string[] args)
        {
            server = new Server(8080);

            Console.CancelKeyPress += Console_CancelKeyPress;
            server.AcceptClients();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            server.Stop();
        }
    }
}
