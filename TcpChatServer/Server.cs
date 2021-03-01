using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace TcpChatServer
{
    class Server
    {
        private readonly int port;
        private readonly TcpListener listener;

        private readonly List<ClientSession> sessions;

        private readonly object removeSessionLock = new object();
        
        public Server(int port)
        {
            this.port = port;

            //Сервер будет прослушивать подключения на всех сетевых интерфейсах компьютера.
            listener = new TcpListener(IPAddress.Any, port);

            sessions = new List<ClientSession>();
        }

        public void AcceptClients()
        {
            listener.Start();

            Console.WriteLine($"Server started on {IPAddress.Any}:{port}");

            try
            {
                while (true)
                {
                    TcpClient tcpClient = listener.AcceptTcpClient();
                    ClientSession session = new ClientSession(this, tcpClient);

                    sessions.Add(session);
                    session.AuthorizeUserAndProcessMessagesAsync();
                }
            }
            finally
            {
                Stop();
            }
        }

        public void Stop()
        {
            listener.Stop();

            foreach (ClientSession session in sessions)
            {
                session.Close();
            }
        }

        public void BroadcastMessage(ClientSession senderSession, string message)
        {
            foreach (ClientSession session in sessions)
            {
                if (session != senderSession)
                {
                    session.SendMessageToUser(message);
                }
            }

            Console.WriteLine(message);
        }

        public void RemoveSession(ClientSession session)
        {
            lock (removeSessionLock)
            {
                sessions.Remove(session);
            }
        }
    }
}
