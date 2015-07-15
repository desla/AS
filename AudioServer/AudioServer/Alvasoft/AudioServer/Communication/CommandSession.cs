using System;
using System.Runtime.InteropServices;
using Alvasoft.Tcp;
using log4net;

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
            Logger.Info("Получена команда звукового сообщения");

            var message = new SoundCommand();
            var size = Marshal.SizeOf(typeof (SoundCommand));
            var pHandle = Marshal.AllocHGlobal(size);
            Marshal.Copy(aBuffer, 0, pHandle, aBufferSize);
            Marshal.PtrToStructure(pHandle, message);
            Marshal.FreeHGlobal(pHandle);

            var channels = new int[message.channelCount];
            for (var i = 0; i < message.channelCount; ++i) {
                channels[i] = message.channels[i];
            }

            var fileNames = new string[message.fileCount];
            for (var i = 0; i < message.fileCount; ++i) {
                fileNames[i] = new string(message.files, i*20, 20).Trim('\0');                
            }

            if (callback != null) {
                callback.OnPlaySound(this, channels, message.priority, fileNames);
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
    }
}