namespace NetworKit.MessageHandler.Delegate
{
    using System;

    public class NetworkServerDelegateMessageHandler : INetworkServerMessageHandler
    {
        #region fields

        private Action<IRemoteConnection, string> _clientConnection;
        private Action<IRemoteConnection, string> _newMessage;
        private Action<IRemoteConnection, string> _clientDisconnection;

        #endregion

        #region constructors

        public NetworkServerDelegateMessageHandler(Action<IRemoteConnection,string> clientConnection, Action<IRemoteConnection, string> newMessage, Action<IRemoteConnection, string> clientDisconnection)
        {
            if (clientConnection == null)
            {
                throw new ArgumentNullException(nameof(clientConnection));
            }

            if (newMessage == null)
            {
                throw new ArgumentNullException(nameof(newMessage));
            }

            if (clientDisconnection == null)
            {
                throw new ArgumentNullException(nameof(clientDisconnection));
            }

            _clientConnection = clientConnection;
            _newMessage = newMessage;
            _clientDisconnection = clientDisconnection;

            //_clientConnection = clientConnection ?? throw new ArgumentNullException(nameof(clientConnection));
            //_newMessage = newMessage ?? throw new ArgumentNullException(nameof(newMessage));
            //_clientDisconnection = clientDisconnection ?? throw new ArgumentNullException(nameof(clientDisconnection));
        }

        #endregion

        #region methods

        public void OnNewConnection(IRemoteConnection client, string connectionRequest)
        {
            _clientConnection(client, connectionRequest);
        }

        public void OnMessageReceived(IRemoteConnection client, string message)
        {
            _newMessage(client, message);
        }

        public void OnClientDisconnection(IRemoteConnection client, string justification)
        {
            _clientDisconnection(client, justification);
        }

        #endregion
    }
}
