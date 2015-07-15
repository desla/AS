using System;

namespace Alvasoft.ConnectionHolder
{
    /// <summary>
    /// Интерфейс Callback'ов для ConectionHolder.
    /// </summary>
    public interface ConnectionHolderCallback
    {
        /// <summary>
        /// Вызывается в случае установления соединения с сервером.
        /// </summary>
        /// <param name="aConnection">Текущий ConnectionHolder.</param>
        void OnConnected(ConnectionHolderBase aConnection);

        /// <summary>
        /// Вызывается в случае отключения от сервера.
        /// </summary>
        /// <param name="aConnection">Текущий ConnectionHolder.</param>
        void OnDisconnected(ConnectionHolderBase aConnection);

        /// <summary>
        /// Вызывается в случае обработки ошибки.
        /// </summary>
        /// <param name="aConnection">Текущий ConnectionHolder.</param>
        /// <param name="aError">Ошибка.</param>
        void OnError(ConnectionHolderBase aConnection, Exception aError);
    }
}
