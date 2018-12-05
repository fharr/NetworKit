namespace NetworKit.Reactive
{
    using System;
    using System.Reactive.Subjects;

    public class ReactiveNetworkClientMessageHandler : INetworkClientMessageHandler
    {
        #region fields

        private Subject<string> _newMessage = new Subject<string>();
        private Subject<string> _serverDisconnection = new Subject<string>();

        #endregion

        #region properties

        public IObservable<string> NewMessage { get { return _newMessage; } }
        public IObservable<string> ServerDisconnection { get { return _serverDisconnection; } }

        #endregion

        #region methods

        public void OnMessageReceived(string message)
        {
            _newMessage.OnNext(message);
        }

        public void OnServerDisconnection(string justfication)
        {
            _serverDisconnection.OnNext(justfication);
        }

        #endregion
    }
}
