using System;
using System.Net.Sockets;
using System.Text;

namespace TcpChatServer
{
    //TODO: задокументировать исключения.
    sealed class MessagesReader
    {
        private readonly NetworkStream stream;

        public MessagesReader(NetworkStream stream)
        {
            this.stream = stream;
        }

        public int ReadMessage(out string message)
        {
            int messageLength = ReadMessageLength();
            int bytesReadCount = ReadMessage(out message, messageLength);

            return bytesReadCount;
        }

        private int ReadMessageLength()
        {
            byte[] messageLengthBytes = new byte[sizeof(int)];

            //TODO: обрабатывать ошибку если читаются меньше sizeof(int) байт.
            stream.Read(messageLengthBytes, 0, messageLengthBytes.Length);

            return ConvertNetworkOrderBytesToInt32(messageLengthBytes);
        }

        private int ConvertNetworkOrderBytesToInt32(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToInt32(bytes, 0);
        }

        private int ReadMessage(out string message, int messageLength)
        {
            byte[] recieveBuffer = new byte[messageLength];

            int bytesReadCount = stream.Read(recieveBuffer, 0, recieveBuffer.Length);
            message = Encoding.UTF8.GetString(recieveBuffer, 0, bytesReadCount);

            return bytesReadCount;
        }
    }
}
