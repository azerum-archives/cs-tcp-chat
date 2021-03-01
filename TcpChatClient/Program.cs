using ChatProtocol;

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TcpChatClient
{
    class Program
    {
        private const int serverPort = 8080;

        private static TcpClient tcpClient;
        private static ChatClient chatClient;

        static void Main(string[] args)
        {
            Console.Write("Enter server IP to join a chat > ");
            IPAddress serverIP = IPAddress.Parse(Console.ReadLine());

            tcpClient = new TcpClient();
            tcpClient.Connect(serverIP, serverPort);

            chatClient = new ChatClient(tcpClient.GetStream());

            Console.Write("Enter your nickname > ");
            string nickname = Console.ReadLine();

            chatClient.SendMessage(nickname);

            Console.CancelKeyPress += Console_CancelKeyPress;

            Thread recieveMessagesThread = new Thread(RecieveMessages);
            recieveMessagesThread.IsBackground = true;

            try
            {
                recieveMessagesThread.Start();
                SendMessages();
            }
            finally
            {
                tcpClient.Close();
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            tcpClient.Close();
        }

        public static void RecieveMessages()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        string message = chatClient.RecieveMessage();

                        if (message == null)
                        {
                            break;
                        }

                        Console.WriteLine(message);
                    }
                    catch (MalformedMessageException)
                    {
                        ;
                    }
                }
            }
            catch (IOException ex) when (ex.InnerException is SocketException socketEx &&
                                         socketEx.ErrorCode == (int)SocketError.ConnectionReset)
            {
                Console.Error.WriteLine("Error: Lost connection with server.");
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
        }

        public static void SendMessages()
        {
            try
            {
                while (true)
                {
                    string message = Console.ReadLine();
                    chatClient.SendMessage(message);
                }
            }
            catch (IOException ex) when (ex.InnerException is SocketException socketEx &&
                                         socketEx.ErrorCode == (int)SocketError.ConnectionReset)
            {
                Console.Error.WriteLine("Error: Lost connection with server.");
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
