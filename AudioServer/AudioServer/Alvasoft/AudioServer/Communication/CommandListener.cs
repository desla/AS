using Alvasoft.Tcp;

namespace Alvasoft.AudioServer.Communication
{
    /// <summary>
    /// Слушатель команд.
    /// </summary>
    public class CommandListener : AbstractServerListener
    {
        /// <inheritdoc />
        protected override AbstractClientSession CreateSession(ClientConnection aClientConnection)
        {
            return new CommandSession(aClientConnection);
        }
    }
}