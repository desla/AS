using System;
using System.Runtime.InteropServices;
using Alvasoft.Tcp;
using log4net;
using System.Net;
using System.Text;

namespace Alvasoft.AudioServer.Communication
{

    /// <summary>
    /// Сессия команд.
    /// </summary>
    public class CommandSession : AbstractClientSession, ClientConnectionCallback
    {
        private static readonly ILog Logger = LogManager.GetLogger("CommandSession");

        private CommandSessionCallback callback;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="aClientConnection">Клиентское подключение.</param>
        public CommandSession(ClientConnection aClientConnection)
            : base(aClientConnection)
        {
            callback = null;
            GetClientConnection().SetCallback(this);
        }

        /// <inheritdoc />
        public void OnReceivedFrom(ClientConnection aClientConnection, byte[] aBuffer, int aBufferSize)
        {
            try {
                Logger.Info("Получена команда звукового сообщения");
                Logger.Info("Разбор сообщения...");                

                var message = new SoundCommand();
                var size = Marshal.SizeOf(typeof(SoundCommand));
                Logger.Info("Размер структуры звукового сообщения: " + size);
                var pHandle = Marshal.AllocHGlobal(size);
                Marshal.Copy(aBuffer, 0, pHandle, aBufferSize);
                Marshal.PtrToStructure(pHandle, message);
                Marshal.FreeHGlobal(pHandle);

                Logger.Info("Разбор сообщения завершен.");
                message.priority = ReverceBits(message.priority);
                Logger.Info("Приоритет: " + message.priority);
                message.channelCount = ReverceBits(message.channelCount);
                Logger.Info("Число каналов для объявления:" + message.channelCount);
                message.fileCount = ReverceBits(message.fileCount);
                Logger.Info("Число файлов для воспроизведения:" + message.fileCount);
                var channelsStr = string.Empty;
                for (var i = 0; i < message.channelCount && i < 15; ++i) {
                    message.channels[i] = ReverceBits(message.channels[i]);
                    channelsStr += message.channels[i] + " ";
                }
                Logger.Info("Список каналов для воспроизведения: " + channelsStr);

                var filesStr = Encoding.ASCII.GetString(message.files);
                var files = string.Empty;
                for (var i = 0; i < message.fileCount && i < 5; ++i) {
                    files += "\"" + filesStr.Substring(i * 20, 20).Trim('\0') + "\" ";
                }
                Logger.Info("Список файлов для воспроизведения: " + files);

                if (message.channelCount > 0 && message.fileCount > 0 &&
                    message.channelCount <= 15 && message.fileCount <= 5) {
                    var channels = new int[message.channelCount];
                    for (var i = 0; i < message.channelCount; ++i) {
                        channels[i] = message.channels[i];
                    }

                    var fileNames = new string[message.fileCount];
                    var messageFiles = Encoding.ASCII.GetString(message.files);
                    for (var i = 0; i < message.fileCount; ++i) {
                        fileNames[i] = messageFiles.Substring(i * 20, 20).Trim('\0');
                    }

                    if (callback != null) {
                        callback.OnPlaySound(this, channels, message.priority, fileNames);
                    }
                }
            }
            catch (Exception ex) {
                Logger.Error("Ошибка при обработке сообщения: " + ex.Message);
            }
        }

        /// <summary>
        /// Инициализирует интерфейс обратных вызовов.
        /// </summary>
        /// <param name="aCallback">Интерфейс обратных вызовов.</param>
        public void SetCallback(CommandSessionCallback aCallback)
        {
            callback = aCallback;
        }

        /// <summary>
        /// переворачивает биты в числе.
        /// </summary>
        /// <param name="aValue">Значение.</param>
        /// <returns>Результат.</returns>
        private int ReverceBits(int aValue)
        {            
            return IPAddress.NetworkToHostOrder(aValue);
        }
    }
}