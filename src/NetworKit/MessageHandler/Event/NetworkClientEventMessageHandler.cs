namespace NetworKit.MessageHandler.Event
{
    public class NetworkClientEventMessageHandler : INetworkClientMessageHandler
    {
        #region events

        public event ClientMessageHandler ServerDisconnected;
        public event ClientMessageHandler MessageReceived;

        #endregion

        #region methods

        public void OnMessageReceived(string message)
        {
            this.MessageReceived?.Invoke(message);
        }

        public void OnServerDisconnection(string justfication)
        {
            this.ServerDisconnected?.Invoke(justfication);
        }

        #endregion
    }
}
