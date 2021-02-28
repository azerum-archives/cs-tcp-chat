using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpChatServer
{
    //TODO: задокументировать исключения!
    class MessagesWriter
    {
        private readonly NetworkStream stream;
        private readonly SemaphoreSlim writeSemaphore = new SemaphoreSlim(1, 1);

        public MessagesWriter(NetworkStream stream)
        {
            this.stream = stream;
        }

        public async Task SendMessageAsync(string message)
        {
            byte[] messageBody = Encoding.UTF8.GetBytes(message);
            int messageBodySize = messageBody.Length;

            byte[] messageBodySizeAsBytes = ConvertInt32ToNetworkOrderBytes(messageBodySize);

            await writeSemaphore.WaitAsync();

            try
            {
                await stream.WriteAsync(messageBodySizeAsBytes, 0, messageBodySizeAsBytes.Length);

                //UTF8 не зависит от порядка бит в байте, поэтому байты, полученные из
                //строки при помощи Encoding.UTF8, не нужно переводить в big-endian
                //перед отправкой по сети.
                await stream.WriteAsync(messageBody, 0, messageBody.Length);
            }
            finally
            {
                writeSemaphore.Release();
            }
        }

        private byte[] ConvertInt32ToNetworkOrderBytes(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }
    }
}
