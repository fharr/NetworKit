namespace NetworKit.MessageHandler.Queue
{
    using System.Collections.Concurrent;

    public class NetworkClientQueueMessageHandler : INetworkClientMessageHandler
    {
        #region properties

        public ConcurrentQueue<string> NewMessages { get; }

        public bool IsDisconnected { get; private set; }
        public string Justification { get; private set; }

        #endregion

        #region constructors

        public NetworkClientQueueMessageHandler()
        {
            this.NewMessages = new ConcurrentQueue<string>();
        }

        #endregion

        #region methods

        public void OnMessageReceived(string message)
        {
            this.NewMessages.Enqueue(message);
        }

        public void OnServerDisconnection(string justfication)
        {
            this.IsDisconnected = true;
            this.Justification = justfication;
        }

        #endregion
    }
}
