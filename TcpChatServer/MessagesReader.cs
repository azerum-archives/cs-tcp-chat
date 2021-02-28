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
            int messageBodySize = ReadMessageBodySize();
            int bytesReadCount = ReadMessageBody(out message, messageBodySize);

            return bytesReadCount;
        }

        private int ReadMessageBodySize()
        {
            byte[] sizeBytes = new byte[sizeof(int)];

            //TODO: обрабатывать ошибку если читаються на все sizeof(int) байт.
            stream.Read(sizeBytes, 0, sizeBytes.Length);

            //При передаче по сети используется порядок байт big-endian.
            //В некоторых системах, например в Windows, порядок байт обратный - little-endian.
            //Изменяем порядок полученых байт, если это нужно для нашей системы.
            return ConvertNetworkOrderBytesToInt32(sizeBytes);
        }

        private int ConvertNetworkOrderBytesToInt32(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToInt32(bytes, 0);
        }

        private int ReadMessageBody(out string messageBody, int messageBodySize)
        {
            byte[] recieveBuffer = new byte[messageBodySize];

            int bytesReadCount = stream.Read(recieveBuffer, 0, recieveBuffer.Length);
            messageBody = Encoding.UTF8.GetString(recieveBuffer, 0, bytesReadCount);

            return bytesReadCount;
        }
    }
}
