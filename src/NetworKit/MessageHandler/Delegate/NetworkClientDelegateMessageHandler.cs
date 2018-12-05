namespace NetworKit.MessageHandler.Delegate
{
    using System;

    public class NetworkClientDelegateMessageHandler : INetworkClientMessageHandler
    {
        #region fields

        private Action<string> _newMessage;
        private Action<string> _serverDisconnection;

        #endregion

        #region constructors

        public NetworkClientDelegateMessageHandler(Action<string> newMessage, Action<string> serverDisconnection)
        {
            if(newMessage == null)
            {
                throw new ArgumentNullException(nameof(newMessage));
            }

            if (serverDisconnection == null)
            {
                throw new ArgumentNullException(nameof(serverDisconnection));
            }

            _newMessage = newMessage;
            _serverDisconnection = serverDisconnection;

            //_newMessage = newMessage ?? throw new ArgumentNullException(nameof(newMessage));
            //_serverDisconnection = serverDisconnection ?? throw new ArgumentNullException(nameof(serverDisconnection));
        }

        #endregion

        #region methods

        public void OnMessageReceived(string message)
        {
            _newMessage(message);
        }

        public void OnServerDisconnection(string justfication)
        {
            _serverDisconnection(justfication);
        }

        #endregion
    }
}
