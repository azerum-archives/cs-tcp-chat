using ChatProtocol;

using System.Net.Sockets;
using System.Threading;

namespace TcpChatServer
{
    class ClientSession
    {
        private Server server;

        private TcpClient tcpClient;
        private string clientNickname;

        private ChatClient chatClient;

        public ClientSession(Server server, TcpClient tcpClient)
        {
            this.server = server;
            this.tcpClient = tcpClient;

            chatClient = new ChatClient(tcpClient.GetStream());
        }

        public void AuthorizeUserAndProcessMessagesAsync()
        {
            Thread thread = new Thread(AuthorizeUserAndProcessMessages);
            thread.IsBackground = true;

            thread.Start();
        }

        private void AuthorizeUserAndProcessMessages()
        {
            clientNickname = chatClient.RecieveMessage();
            server.BroadcastMessage(this, $"{clientNickname} has joined the chat");

            while (true)
            {
                string message = chatClient.RecieveMessage();

                if (message == null)
                {
                    //TODO: уведомить других пользователей об отключении этого клиента.
                    break;
                }

                server.BroadcastMessage(this, $"{clientNickname}: {message}");
            }
        }

        public void SendMessageToUser(string message)
        {
            chatClient.SendMessage(message);
        }
    }
}
