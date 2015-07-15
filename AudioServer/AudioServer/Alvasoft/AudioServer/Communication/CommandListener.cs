using Alvasoft.Tcp;

namespace Alvasoft.AudioServer.Communication
{
    /// <summary>
    /// Слушатель команд.
    /// </summary>
    public class CommandListener : AbstractServerListener
    {
        private bool bitReverce = true;

        public CommandListener(bool aBitReverce)
        {
            bitReverce = aBitReverce;
        }

        /// <inheritdoc />
        protected override AbstractClientSession CreateSession(ClientConnection aClientConnection)
        {
            return new CommandSession(aClientConnection, bitReverce);
        }
    }
}