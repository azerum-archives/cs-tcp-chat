using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace TcpChatServer
{
    class ClientSession
    {
        private Server server;

        private TcpClient tcpClient;
        private string clientNickname;

        private MessagesReader messagesReader;
        private MessagesWriter messageWriter;

        public ClientSession(Server server, TcpClient tcpClient)
        {
            this.server = server;
            this.tcpClient = tcpClient;

            NetworkStream stream = tcpClient.GetStream();

            messagesReader = new MessagesReader(stream);
            messageWriter = new MessagesWriter(stream);
        }

        public void AuthorizeUserAndProcessMessagesAsync()
        {
            Thread thread = new Thread(AuthorizeUserAndProcessMessages);
            thread.IsBackground = true;

            thread.Start();
        }

        private void AuthorizeUserAndProcessMessages()
        {
            messagesReader.ReadMessage(out clientNickname);
            server.BroadcastMessageAsync(this, $"{clientNickname} has joined the chat").Forget();

            while (true)
            {
                int bytesReadCount = messagesReader.ReadMessage(out string message);

                if (bytesReadCount == 0)
                {
                    //TODO: уведомить других пользователей об отключении этого клиента.
                    break;
                }

                server.BroadcastMessageAsync(this, $"{clientNickname}: {message}").Forget();
            }
        }

        public async Task SendMessageToUserAsync(string message)
        {
            await messageWriter.SendMessageAsync(message);
        }
    }
}
