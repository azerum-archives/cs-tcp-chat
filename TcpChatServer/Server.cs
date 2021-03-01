using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace TcpChatServer
{
    class Server
    {
        private int port;
        private List<ClientSession> sessions;

        private object removeSessionLock = new object();

        public Server(int port)
        {
            this.port = port;
            sessions = new List<ClientSession>();
        }

        public void AcceptClients()
        {
            //Прослушиваем подключения на всех сетевых интерфейсах компьютера.
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            Console.WriteLine($"Server started on {IPAddress.Any}:{port}");

            while (true)
            {
                TcpClient tcpClient = listener.AcceptTcpClient();
                ClientSession session = new ClientSession(this, tcpClient);

                sessions.Add(session);
                session.AuthorizeUserAndProcessMessagesAsync();
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
