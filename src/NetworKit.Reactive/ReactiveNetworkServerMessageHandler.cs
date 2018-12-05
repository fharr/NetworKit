namespace NetworKit.Reactive
{
    using System;
    using System.Reactive.Subjects;

    public class ReactiveNetworkServerMessageHandler : INetworkServerMessageHandler
    {
        #region fields

        private Subject<NetworkMessage> _newConnection = new Subject<NetworkMessage>();
        private Subject<NetworkMessage> _newMessage = new Subject<NetworkMessage>();
        private Subject<NetworkMessage> _clientDisconnection = new Subject<NetworkMessage>();

        #endregion

        #region properties

        public IObservable<NetworkMessage> NewConnection { get { return _newConnection; } }
        public IObservable<NetworkMessage> NewMessage { get { return _newMessage; } }
        public IObservable<NetworkMessage> ClientDisconnection { get { return _clientDisconnection; } }

        #endregion

        #region methods

        public void OnNewConnection(IRemoteConnection client, string connectionRequest)
        {
            _newConnection.OnNext(new NetworkMessage(client, connectionRequest));
        }

        public void OnMessageReceived(IRemoteConnection client, string message)
        {
            _newMessage.OnNext(new NetworkMessage(client, message));
        }

        public void OnClientDisconnection(IRemoteConnection client, string justification)
        {
            _clientDisconnection.OnNext(new NetworkMessage(client, justification));
        }

        #endregion
    }
}
