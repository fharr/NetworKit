namespace NetworKit.MessageHandler.Queue
{
    using System.Collections.Concurrent;

    public class NetworkClientQueueMessageHandler : INetworkClientMessageHandler
    {
        #region fields

        private DisconnectionMessage _disconnectionMessage;

        #endregion

        #region properties

        public ConcurrentQueue<string> NewMessages { get; }

        #endregion

        #region constructors

        public NetworkClientQueueMessageHandler()
        {
            this.NewMessages = new ConcurrentQueue<string>();
        }

        #endregion

        #region methods

        public bool IsDeconnectedByServer(out string justification)
        {
            if(_disconnectionMessage != null)
            {
                justification = _disconnectionMessage.Justification;
                return true;
            }

            justification = null;
            return false;
        }

        public void OnMessageReceived(string message)
        {
            this.NewMessages.Enqueue(message);
        }

        public void OnServerDisconnection(string justfication)
        {
            _disconnectionMessage = new DisconnectionMessage(justfication);
        }

        #endregion

        #region internal classes

        private class DisconnectionMessage
        {
            public string Justification { get; }

            public DisconnectionMessage(string justification)
            {
                this.Justification = justification;
            }
        }

        #endregion
    }
}
