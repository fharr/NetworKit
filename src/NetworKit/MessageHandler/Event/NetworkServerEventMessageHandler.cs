namespace NetworKit.MessageHandler.Event
{
    public class NetworkServerEventMessageHandler : INetworkServerMessageHandler
    {
        #region events

        public event ServerMessageHandler ClientDisconnected;
        public event ServerMessageHandler MessageReceived;
        public event ServerMessageHandler NewConnection;

        #endregion

        #region methods

        public void OnClientDisconnection(IRemoteConnection client, string justification)
        {
            this.ClientDisconnected?.Invoke(client, justification);
        }

        public void OnMessageReceived(IRemoteConnection client, string message)
        {
            this.MessageReceived?.Invoke(client, message);
        }

        public void OnNewConnection(IRemoteConnection client, string connectionRequest)
        {
            this.NewConnection?.Invoke(client, connectionRequest);
        }

        #endregion
    }
}
