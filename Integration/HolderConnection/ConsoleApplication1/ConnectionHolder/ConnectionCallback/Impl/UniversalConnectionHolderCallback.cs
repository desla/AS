using System;

namespace StableConnection.ConnectionHolder.ConnectionCallback.Impl
{
    /// <summary>
    /// Универсальный callback для всех типов Holder'ов.
    /// </summary>
    public class UniversalConnectionHolderCallback : IConnectionHolderCallback
    {
        /// <summary>
        /// Вызывается в случае установления связи с сервером.
        /// Выводит информацию в лог.
        /// </summary>
        /// <param name="aConnection">Текущий ConnectionHolder.</param>
        public void OnConnected(ConnectionHolderBase aConnection)
        {
            Console.WriteLine(aConnection.GetHolderName() + " cоединение установлено.");
        }

        /// <summary>
        /// Вызывается в случае потери соединения.
        /// Выводит информацию в лог.
        /// </summary>
        /// <param name="aConnection">Текущий ConnectionHolder.</param>
        public void OnDisconnected(ConnectionHolderBase aConnection)
        {
            Console.WriteLine(aConnection.GetHolderName() + " cоединение потеряно.");
        }

        /// <summary>
        /// Вызывается при обработке ошибок.
        /// </summary>
        /// <param name="aConnection">Текущий ConnectionHolder.</param>
        /// <param name="aError">Ошибка.</param>
        public void OnError(ConnectionHolderBase aConnection, Exception aError)
        {
            Console.WriteLine(aConnection.GetHolderName() + " ошибка: " + aError.Message);
        }
    }
}
