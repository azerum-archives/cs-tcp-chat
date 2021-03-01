using System;
using System.Net.Sockets;
using System.Text;

namespace ChatProtocol
{
    public class ChatClient
    {
        private readonly NetworkStream stream;

        /// <summary>
        /// <para>
        /// Создай клиент для отправки и получения чат-сообщений
        /// по протоколу TCP по сетевому потоку.
        /// </para>
        /// <para>
        /// Подразумеваеться, что поток <paramref name="stream"/> открыт.
        /// </para>
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public ChatClient(NetworkStream stream)
        {
            this.stream = stream ?? throw new ArgumentNullException("stream");
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

        /// <summary>
        /// Отправляет чат-сообщение, которое можно прочитать при помощи
        /// <see cref="ChatClient.RecieveMessage"/>.
        /// </summary>
        /// <param name="message"> Сообщение, которое будет отправлено. </param>
        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="ObjectDisposedException"/>
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

        /// <summary>
        /// <para>
        /// Получает чат-сообщение. Если доступных сообщений нет, 
        /// то метод блокируеться в ожидании нового сообщения.
        /// </para>
        /// <para>
        /// Возвращает строку сообщения, или <c>null</c>, если подключение
        /// по сети закрыто.
        /// </para>
        /// </summary>
        /// <exception cref="MalformedMessageException"/>
        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public string RecieveMessage()
        {
            ushort? messageLength = ReadMessageLength();

            if (messageLength == null)
            {
                return null;
            }

            //Оптимизация: при длине сообщения 0 можно не вызывать ReadMessage(),
            //а сразу возвращать пустую строку.
            if (messageLength == 0)
            {
                return "";
            }

            return ReadMessage(messageLength.Value);
        }


        /// <summary>
        /// <para> Читает заголовок сообщения с его длиной из сетевого потока. </para>
        /// <para>
        /// Возвращает длину сообщения, или <c>null</c>, если соеднинение по сети
        /// закрыто.
        /// </para>
        /// </summary>
        /// <exception cref="MalformedMessageException"/>
        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        private ushort? ReadMessageLength()
        {
            byte[] messageLengthBytes = new byte[sizeof(ushort)];
            int bytesReadCount = stream.Read(messageLengthBytes, 0, messageLengthBytes.Length);

            if (bytesReadCount == 0)
            {
                return null;
            }

            if (bytesReadCount < sizeof(ushort))
            {
                throw new MalformedMessageException($"Сообщение должно содержать {sizeof(ushort)} байт заголовка.");
            }

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(messageLengthBytes);
            }

            return BitConverter.ToUInt16(messageLengthBytes, 0);
        }


        /// <summary>
        /// <para>
        /// Читает следующие <paramref name="messageLength"/> байт из сетевого потока и
        /// декодирует их в сообщение.
        /// </para>
        /// <para> Возвращает не-<c>null</c> строку сообщения. </para>
        /// </summary>
        /// <exception cref="MalformedMessageException"/>
        /// <exception cref="System.IO.IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        private string ReadMessage(ushort messageLength)
        {
            byte[] messageBytes = new byte[messageLength];
            int bytesReadCount = stream.Read(messageBytes, 0, messageBytes.Length);

            if (bytesReadCount < messageLength)
            {
                throw new MalformedMessageException("Длина сообщения не соответсвтует длине, указанной в его заголоке.");
            }

            string message = Encoding.UTF8.GetString(messageBytes, 0, messageBytes.Length);
            return message;
        }
    }
}
