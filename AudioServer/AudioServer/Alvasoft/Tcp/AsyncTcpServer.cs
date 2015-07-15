using System.Net;
using Alvasoft.Utils.Activity;

namespace Alvasoft.Tcp
{
    /// <summary>
    /// Интерфейс асинхронного TCP-сервера.
    /// </summary>
    public interface AsyncTcpServer : Initializable
    {
        /// <summary>
        /// Возвращает прослушиваемый интерфейс и номер порта.
        /// </summary>
        /// <returns>Прослушиваемый интерфейс и номер порта.</returns>
        IPEndPoint GetListenEndPoint();

        /// <summary>
        /// Инициализирует интерфейс обратных вызовов.
        /// </summary>
        /// <param name="aCallback">Интерфейс обратных вызовов.</param>
        void SetCallback(AsyncTcpServerCallback aCallback);
    }
}