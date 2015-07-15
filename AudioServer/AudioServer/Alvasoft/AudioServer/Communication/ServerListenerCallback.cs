namespace Alvasoft.AudioServer.Communication
{
    /// <summary>
    /// Интерфейс для обратной связи слушатля.
    /// </summary>
    public interface ServerListenerCallback
    {
        /// <summary>
        /// Срабатывает при создании клиентской сессии.
        /// </summary>
        /// <param name="aServerListener">Слушатель сервера.</param>
        /// <param name="aClientSession">Клиентскае сессия.</param>
        void OnCreatedSession(AbstractServerListener aServerListener, AbstractClientSession aClientSession);

        /// <summary>
        /// Срабатывает при отключении клиента.
        /// </summary>
        /// <param name="aServerListener">Слушатель.</param>
        /// <param name="aClientSession">Клиентская сессия.</param>
        void OnCloseConnection(AbstractServerListener aServerListener, AbstractClientSession aClientSession);
    }
}