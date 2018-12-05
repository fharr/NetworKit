namespace NetworKit.Tcp
{
    using NetworKit.Tcp.Utils;
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using System.Timers;

    public class TcpNetworkServer : INetworkServer
    {
        #region fields

        private readonly Timer _timer;
        private readonly ConcurrentDictionary<IRemoteConnection, TcpRemoteConnection> _clients;

        private TcpListener _listener;

        #endregion

        #region properties

        public bool IsListening { get { return _timer.Enabled; } }

        public INetworkServerSettings Settings { get { return this.TcpSettings; } }
        public TcpNetworkServerSettings TcpSettings { get; }

        public IReadOnlyCollection<IRemoteConnection> Clients { get; }

        #endregion

        #region constructors

        public TcpNetworkServer(INetworkServerMessageHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _timer = new Timer();
            _timer.AutoReset = true;
            _timer.Elapsed += this.Tick;

            _clients = new ConcurrentDictionary<IRemoteConnection, TcpRemoteConnection>();

            this.Clients = new MyReadOnlyCollection(_clients);
            this.TcpSettings = new TcpNetworkServerSettings { Handler = handler };
        }

        #endregion

        #region public methods

        public async void StartListening(Func<IRemoteConnection, string, ConnectionStatus> validateConnection = null)
        {
            _listener = new TcpListener(IPAddress.Any, this.Settings.LocalPort);
            _listener.Start();

            _timer.Enabled = true;

            while (true)
            {
                try
                {
                    // Accepts Tcp Connection
                    var client = await _listener.AcceptTcpClientAsync();

                    this.ValidateNewConnection(client, validateConnection);
                }
                catch (ObjectDisposedException)
                {
                    // The server has been stopped. Nothing to do except leave the loop.
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine("---------------------- Error while listening new connections");
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        public async Task BroadcastAsync(string message)
        {
            var clients = _clients.Values;

            var sending = new List<Task>(clients.Count);

            foreach(var client in clients)
            {
                sending.Add(this.SendToAsync(client, message));
            }

            await Task.WhenAll(sending);
        }

        public async Task SendToAsync(IRemoteConnection remote, string message)
        {
            TcpRemoteConnection tcpRemote;

            if (!_clients.TryGetValue(remote, out tcpRemote))
            {
                throw new Exception("This remote is not connected to the server");
            }

            try
            {
                await tcpRemote.SendAsync(new TcpMessage(TcpNetworkCommand.Message, message));
            }
            catch(Exception e)
            {
                // TODO : log exception ??
                this.TcpSettings.Handler?.OnClientDisconnection(remote, "ConnectionLost");
            }
        }

        public async Task CloseConnectionAsync(IRemoteConnection remote, string justification = null)
        {
            TcpRemoteConnection tcpRemote;

            if (!_clients.TryRemove(remote, out tcpRemote))
            {
                throw new Exception("This remote is not connected to the server");
            }

            await tcpRemote.SendAsync(new TcpMessage(TcpNetworkCommand.Disconnection, justification));

            this.TcpSettings.Handler?.OnClientDisconnection(remote, "ConnectionLost");
        }

        public async Task StopListening()
        {
            _listener.Stop();
            _timer.Enabled = false;

            await Task.WhenAll(_clients.Keys.Select(client => this.CloseConnectionAsync(client, "Server closed")));
            
            _clients.Clear();
        }

        #endregion

        #region private methods

        private async void ValidateNewConnection(TcpClient client, Func<IRemoteConnection, string, ConnectionStatus> validateConnection)
        {
            var remote = new TcpRemoteConnection(client, this.TcpSettings);

            try
            {
                // Waits for the connection request
                var request = await remote.ReceiveAsync(this.TcpSettings.ConnectionTimeout);

                if (request == null)
                {
                    // TODO: specify timeout reason
                    await remote.SendAsync(new TcpMessage(TcpNetworkCommand.ConnectionDenied));
                    remote.Dispose();
                    return;
                }
                else if (!request.IsValid || request.Command != TcpNetworkCommand.ConnectionRequest)
                {
                    // TODO: specify wrong request reason
                    await remote.SendAsync(new TcpMessage(TcpNetworkCommand.ConnectionDenied));
                    remote.Dispose();
                    return;
                }

                var status = validateConnection?.Invoke(remote, request.InnerMessage);

                if (status?.ConnectionGranted ?? true)
                {
                    await remote.SendAsync(new TcpMessage(TcpNetworkCommand.ConnectionGranted, status?.Status));

                    _clients[remote] = remote;
                    this.TcpSettings.Handler?.OnNewConnection(remote, request.InnerMessage);
                }
                else
                {
                    // TODO: specify connection denied
                    await remote.SendAsync(new TcpMessage(TcpNetworkCommand.ConnectionDenied));
                    remote.Dispose();
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("---------------------- Error while validating a new connection");
                Console.WriteLine(e.StackTrace);
                // TODO: specify connection error
                await remote.SendAsync(new TcpMessage(TcpNetworkCommand.ConnectionDenied));
                remote.Dispose();
            }
        }

        private async void Tick(object sender, ElapsedEventArgs e)
        {
            var receiving = _clients.Values.Select(c => this.ReadClientMessages(c));

            await Task.WhenAll(receiving);
        }

        private async Task ReadClientMessages(TcpRemoteConnection client)
        {
            try
            {
                for (var message = await client.ReceiveAsync(); message != null; message = await client.ReceiveAsync())
                {
                    if (message.IsValid && message.Command == TcpNetworkCommand.Message)
                    {
                        this.TcpSettings.Handler?.OnMessageReceived(client, message.InnerMessage);
                    }
                    else if (message.IsValid && message.Command == TcpNetworkCommand.Disconnection)
                    {
                        this.OnClientDisconnection(client, message.InnerMessage);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("---------------------- Error while listening new messages within the server");
                Console.WriteLine(ex.StackTrace);
                // if connection lost : this.OnClientDisconnection("Connection lost");
                // otherwise : do nothing
            }
        }

        private void OnClientDisconnection(IRemoteConnection client, string justification)
        {
            TcpRemoteConnection tcpRemote;
            _clients.TryRemove(client, out tcpRemote);
            tcpRemote.Dispose();

            this.TcpSettings.Handler?.OnClientDisconnection(client, justification);
        }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _timer.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region private classes

        private class MyReadOnlyCollection : IReadOnlyCollection<IRemoteConnection>
        {
            #region fields

            private readonly ConcurrentDictionary<IRemoteConnection, TcpRemoteConnection> _innerCollection;

            #endregion

            #region properties

            public int Count { get { return _innerCollection.Count; } }

            #endregion

            #region constructors

            public MyReadOnlyCollection(ConcurrentDictionary<IRemoteConnection, TcpRemoteConnection> collection)
            {
                _innerCollection = collection;
            }

            #endregion

            #region methods

            public IEnumerator<IRemoteConnection> GetEnumerator()
            {
                return _innerCollection.Keys.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            #endregion

        }

        #endregion
    }
}
