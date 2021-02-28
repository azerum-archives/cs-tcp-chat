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
            //Разные операционные системы хранят с разным порядком байт.
            //Число int a = 2 в системе с порядком байт от старшего к младшему
            //будет храниться, как 0x00 0x00 0x00 0x02. Такой порядок называется 
            //big-endian.
            //Есть системы, которые хранят байты в порядке от младшего к старшему.
            //int a = 2 будет храниться, как 0x02 0x00 0x00 0x00. Это порядок 
            //little-endian. Windows использует little-endian.

            //Для передачи данных по сети договорились использовать порядок big-endian,
            //поэтому если наша система использует little-endian, то байты нужно 
            //перевести в big-endian.

            //Кодировка UTF8 не зависит от порядка байт, поэтому мы
            //можем сразу использовать байты, полученные из GetBytes()
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            //Для передачи числа порядок байт, возможно, придеться изменить
            int messageLength = messageBytes.Length;
            byte[] messageLengthBytes = ConvertInt32ToNetworkOrderBytes(messageLength);

            await writeSemaphore.WaitAsync();

            try
            {
                //Передаем сообщение в формате
                //| длина сообщения в байтах |             сообщение             |

                await stream.WriteAsync(messageLengthBytes, 0, messageLengthBytes.Length);
                await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
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
