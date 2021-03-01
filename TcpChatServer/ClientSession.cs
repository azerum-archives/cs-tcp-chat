using ChatProtocol;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace TcpChatServer
{
    class ClientSession
    {
        private readonly Server server;

        private readonly TcpClient tcpClient;
        private string clientNickname;

        private readonly ChatClient chatClient;

        private bool opened;
        private readonly object openedLock = new object();

        public ClientSession(Server server, TcpClient tcpClient)
        {
            this.server = server;
            this.tcpClient = tcpClient;

            chatClient = new ChatClient(tcpClient.GetStream());

            opened = false;
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

            opened = true;

            while (opened)
            {
                try
                {
                    string message = chatClient.RecieveMessage();

                    if (message == null)
                    {
                        CloseAndNotifyUsers();
                        break;
                    }

                    server.BroadcastMessage(this, $"{clientNickname}: {message}");
                }
                catch (MalformedMessageException ex)
                {
                    Console.Error.WriteLine($"Error: Recieved malformed message from {clientNickname}.");
                    Console.Error.WriteLine($"Details: {ex.Message}");
                }
                catch (IOException ex) when (ex.InnerException is SocketException socketEx &&
                                         socketEx.ErrorCode == (int)SocketError.ConnectionReset)
                {
                    Console.Error.WriteLine($"Error: client {clientNickname} has reset the connection.");
                    CloseAndNotifyUsers();
                }
                catch (IOException ex)
                {
                    Console.Error.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        public void SendMessageToUser(string message)
        {
            try
            {
                chatClient.SendMessage(message);
            }
            catch (IOException ex) when (ex.InnerException is SocketException socketEx &&
                                         socketEx.ErrorCode == (int)SocketError.ConnectionReset)
            {
                Console.Error.WriteLine($"Error: client {clientNickname} reset connection.");
                CloseAndNotifyUsers();
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
        }

        public void Close()
        {
            lock (openedLock)
            {
                if (opened)
                {
                    opened = false;
                }
                else
                {
                    return;
                }
            }

            server.RemoveSession(this);
            tcpClient.Close();
        }

        public void CloseAndNotifyUsers()
        {
            lock (openedLock)
            {
                if (opened)
                {
                    opened = false;
                }
                else
                {
                    return;
                }
            }

            server.RemoveSession(this);
            server.BroadcastMessage(this, $"{clientNickname} has left the chat.");

            tcpClient.Close();
        }
    }
}
