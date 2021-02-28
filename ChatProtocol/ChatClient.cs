using System;
using System.Net.Sockets;
using System.Text;

namespace ChatProtocol
{
    public class ChatClient
    {
        private readonly NetworkStream stream;

        public ChatClient(NetworkStream stream)
        {
            this.stream = stream;
        }

        #region Формат сообщений
        //Формат сообщений чата:

        //размер в байтах:               2                    n
        //данные:           | длина сообщения в байтах || сообщение |

        //Длина сообщения записывается unsigned short размером 2 байта. 
        //Длина сообщения - это длинна именно сообщения, без учета самого числа, которое хранит длину.
        //Длина может быть 0. Тогда сообщение считается пустой строкой.

        //Используя такой формат мы можем различать отдельные сообщения в потоке.
        //Если бы мы просто читали поток по порциям байт, то мы бы не смогли бы различить
        //несколько сообщений в потоке от одного. 
        //Так, если сообщения придут очень быстро или сервер поздно прочитает поток и в нем 
        //наберется очередь из нескольких  сообщений, то сервер прочитает их как одно.

        //Если при передаче указывать длину сообщения, то ясно, когда одно сообщение заканчивается 
        //и когда начинается другое.
        #endregion

        public void SendMessage(string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            ushort messageLength = (ushort)messageBytes.Length;

            byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);

            //Если наша программа работает на компьютере с порядком байт big-endian
            //(маловероятно), то переводим байты в little-endian перед отправкой по сети.
            //Заметьте, что нам не нужно переводить байты самой строки сообщения в little-endian, 
            //так как мы используем кодировку UTF-8. Магическая кодировка UTF-8 не зависит от порядка
            //байт, так что закодированная ею строка будет правильно интерпретирована на другом компьютере.

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(messageLengthBytes);
            }

            stream.Write(messageLengthBytes, 0, messageLengthBytes.Length);
            stream.Write(messageBytes, 0, messageBytes.Length);
        }

        //TODO: отрефакторить это
        public string RecieveMessage()
        {
            byte[] messageLengthBytes = new byte[sizeof(ushort)];
            int bytesReadCount = stream.Read(messageLengthBytes, 0, messageLengthBytes.Length);

            if (bytesReadCount == 0)
            {
                return null;
            }

            if (bytesReadCount < sizeof(ushort))
            {
                throw new InvalidMessageFormatException($"Сообщение должно содержать {sizeof(ushort)} байт заголовка.");
            }

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(messageLengthBytes);
            }

            ushort messageLength = BitConverter.ToUInt16(messageLengthBytes, 0);

            if (messageLength == 0)
            {
                return "";
            }

            byte[] messageBytes = new byte[messageLength];
            bytesReadCount = stream.Read(messageBytes, 0, messageBytes.Length);

            if (bytesReadCount < messageLength)
            {
                throw new InvalidMessageFormatException("Длина сообщения не соответсвтует длине, указанной в его заголоке.");
            }

            string message = Encoding.UTF8.GetString(messageBytes, 0, messageBytes.Length);
            return message;
        }
    }
}
