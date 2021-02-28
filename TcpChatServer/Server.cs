using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpChatServer
{
    class Server
    {
        private int port;
        private List<ClientSession> sessions;

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

            while (true)
            {
                TcpClient tcpClient = listener.AcceptTcpClient();
                ClientSession session = new ClientSession(this, tcpClient);


            }
        }

        public async Task BroadcastMessageAsync(ClientSession senderSession, string message)
        {
            List<Task> tasks = new List<Task>();

            foreach (ClientSession session in sessions)
            {
                if (session != senderSession)
                {
                    Task task = session.SendMessageToUserAsync(message);
                    tasks.Add(task);
                }
            }

            await Task.WhenAll(tasks);
        }
    }
}
