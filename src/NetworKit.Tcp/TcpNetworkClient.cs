namespace NetworKit.Tcp
{
    using NetworKit.Exceptions;
    using NetworKit.Tcp.Utils;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using System.Timers;

    public class TcpNetworkClient : INetworkClient
    {
        #region fields

        private readonly Timer _timer;

        private TcpRemoteConnection _remoteServer;

        #endregion

        #region properties

        public bool IsConnected { get { return _remoteServer != null; } }

        public INetworkClientSettings Settings { get { return this.TcpSettings; } }
        public TcpNetworkClientSettings TcpSettings { get; }

        #endregion

        #region constructors

        public TcpNetworkClient(INetworkClientMessageHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _timer = new Timer();
            _timer.AutoReset = true;
            _timer.Elapsed += this.Tick;

            this.TcpSettings = new TcpNetworkClientSettings { Handler = handler };
        }

        #endregion

        #region public methods

        public async Task<string> ConnectAsync(string serverAddress, int serverPort, string request = null)
        {
            this.IsAliveAndDisconnected();

            try
            {
                // Initiates the Tcp connection
                var remote = await InitiateTcpConnectionAsync(serverAddress, serverPort);

                // Sends the connection request
                await remote.SendAsync(new TcpMessage(TcpNetworkCommand.ConnectionRequest, request));

                // Waits for the connection response
                var response = await remote.ReceiveAsync(this.TcpSettings.ConnectionTimeout);

                if (response == null)
                {
                    remote.Dispose();
                    throw new ConnectionFailedException(ConnectionFailedType.ConnectionTimeout);
                }
                else if (!response.IsValid)
                {
                    remote.Dispose();
                    throw new ConnectionFailedException(ConnectionFailedType.InvalidResponse, response.InnerMessage);
                }
                else if (response.Command == TcpNetworkCommand.ConnectionDenied)
                {
                    remote.Dispose();
                    throw new ConnectionFailedException(ConnectionFailedType.ConnectionRefused, response.InnerMessage);
                }
                else if (response.Command != TcpNetworkCommand.ConnectionGranted)
                {
                    remote.Dispose();
                    throw new ConnectionFailedException(ConnectionFailedType.UnexpectedResponse, response.ToString());
                }

                _remoteServer = remote;

                // Starts the listening loop
                _timer.Interval = this.TcpSettings.ListeningTick;
                _timer.Enabled = true;

                return response.InnerMessage;
            }
            catch (Exception e) when (!(e is ConnectionFailedException))
            {
                throw new ConnectionFailedException(ConnectionFailedType.Other, e);
            }
        }

        public Task SendAsync(string message)
        {
            this.IsAliveAndConnected();

            return _remoteServer.SendAsync(new TcpMessage(TcpNetworkCommand.Message, message));
        }

        public async Task DisconnectAsync(string justification = null)
        {
            this.IsAliveAndDisconnected();

            await _remoteServer.SendAsync(new TcpMessage(TcpNetworkCommand.Disconnection, justification));

            this.Disconnect();
        }

        #endregion

        #region private methods

        private void IsAlive()
        {
            if (_disposedValue)
            {
                throw new ObjectDisposedException(nameof(TcpNetworkClient));
            }
        }

        private void IsAliveAndConnected()
        {
            this.IsAlive();

            if (!this.IsConnected)
            {
                throw new NotConnectedException();
            }
        }

        private void IsAliveAndDisconnected()
        {
            this.IsAlive();

            if (this.IsConnected)
            {
                throw new AlreadyConnectedException();
            }
        }

        private async Task<TcpRemoteConnection> InitiateTcpConnectionAsync(string serverAddress, int serverPort)
        {
            var client = new TcpClient(new IPEndPoint(IPAddress.Any, this.TcpSettings.LocalPort));

            var connection = client.ConnectAsync(serverAddress, serverPort);
            var timeout = Task.Delay(this.TcpSettings.ConnectionTimeout);

            await Task.WhenAny(connection, timeout);

            if (!connection.IsCompleted)
            {
                client.Close();
                throw new ConnectionFailedException(ConnectionFailedType.RemoteConnectionUnreachable);
            }
            else if (connection.IsFaulted)
            {
                client.Close();
                throw new ConnectionFailedException(ConnectionFailedType.ConnectionRequestFailed, connection.Exception.InnerException);
            }

            return new TcpRemoteConnection(client, this.TcpSettings);
        }

        private async void Tick(object sender, ElapsedEventArgs e)
        {
            try
            {
                for (var message = await _remoteServer.ReceiveAsync(); message != null; message = await _remoteServer.ReceiveAsync())
                {
                    if (message.IsValid && message.Command == TcpNetworkCommand.Message)
                    {
                        this.TcpSettings.Handler?.OnMessageReceived(message.InnerMessage);
                    }
                    else if (message.IsValid && message.Command == TcpNetworkCommand.Disconnection)
                    {
                        this.OnServerDisconnection(message.InnerMessage);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("---------------------- Error while listening new message within the client");
                Console.WriteLine(ex.StackTrace);
                // if connection lost : this.OnServerDisconnection("Connection lost");
                // otherwise : do nothing
            }
        }

        private void OnServerDisconnection(string justification)
        {
            this.Disconnect();

            this.TcpSettings.Handler?.OnServerDisconnection(justification);
        }

        private void Disconnect()
        {
            _timer.Enabled = false;

            _remoteServer.Dispose();
            _remoteServer = null;
        }

        #endregion

        #region IDisposable Support

        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _remoteServer?.Dispose();
                    _timer.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
