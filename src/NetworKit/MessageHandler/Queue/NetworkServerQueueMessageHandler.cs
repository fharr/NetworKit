namespace NetworKit.MessageHandler.Queue
{
    using System.Collections.Concurrent;

    public class NetworkServerQueueMessageHandler : INetworkServerMessageHandler
    {
        #region properties

        public ConcurrentQueue<NetworkMessage> NewClientConnections { get; }
        public ConcurrentQueue<NetworkMessage> NewMessages { get; }
        public ConcurrentQueue<NetworkMessage> LastClientDisconnections { get; }

        #endregion

        #region constructors

        public NetworkServerQueueMessageHandler()
        {
            this.NewClientConnections = new ConcurrentQueue<NetworkMessage>();
            this.NewMessages = new ConcurrentQueue<NetworkMessage>();
            this.LastClientDisconnections = new ConcurrentQueue<NetworkMessage>();
        }

        #endregion

        #region methods

        public void OnNewConnection(IRemoteConnection client, string connectionRequest)
        {
            this.NewClientConnections.Enqueue(new NetworkMessage(client, connectionRequest));
        }

        public void OnMessageReceived(IRemoteConnection client, string message)
        {
            this.NewMessages.Enqueue(new NetworkMessage(client, message));
        }

        public void OnClientDisconnection(IRemoteConnection client, string justification)
        {
            this.LastClientDisconnections.Enqueue(new NetworkMessage(client, justification));
        }

        #endregion
    }
}
