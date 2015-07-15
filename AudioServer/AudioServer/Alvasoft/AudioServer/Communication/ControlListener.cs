using Alvasoft.Tcp;

namespace Alvasoft.AudioServer.Communication
{
    /// <summary>
    /// Слушатель управления.
    /// </summary>
    public class ControlListener : AbstractServerListener
    {
        /// <inheritdoc />
        protected override AbstractClientSession CreateSession(ClientConnection aClientConnection)
        {
            return new ControlSession(aClientConnection);
        }
    }
}