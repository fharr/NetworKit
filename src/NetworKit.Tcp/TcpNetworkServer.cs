namespace NetworKit.Tcp
{
    using NetworKit.Exceptions;
    using NetworKit.Tcp.Utils;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    public class TcpNetworkServer : INetworkServer
    {
        #region properties

        public bool IsListening { get { return this.Listener.Server.IsBound; } }

        public int Port { get { return (this.Listener.LocalEndpoint as IPEndPoint).Port; } }

        public IEnumerable<IRemoteConnection> Clients { get; }

        public TcpConfiguration TcpConfiguration { get; }

        #endregion

        #region private properties

        /// <summary>
        /// The internal tcp listener used to exchange package
        /// </summary>
        private TcpListener Listener { get; }

        /// <summary>
        /// The underlying list of TcpRemoteConnection
        /// </summary>
        private List<TcpRemoteConnection> TcpClients { get; }

        /// <summary>
        /// The delegate executed when a connection request arrives
        /// </summary>
        private ConnectionHandler OnConnectionRequested { get; set; }

        /// <summary>
        /// The delegate executed when a new message is received
        /// </summary>
        private MessageHandler OnMessageReceived { get; set; }

        /// <summary>
        /// The delegate executed when a remote is disconnected
        /// </summary>
        private DisconnectionHandler OnDisconnect { get; set; }

        #endregion

        #region constructors

        public TcpNetworkServer(int port)
        {
            this.Listener = new TcpListener(IPAddress.Any, port);
            this.TcpClients = new List<TcpRemoteConnection>();
            this.TcpConfiguration = new TcpConfiguration();
            this.Clients = new ReadOnlyCollection<TcpRemoteConnection>(this.TcpClients);
        }

        #endregion

        #region methods

        public Task StartListeningAsync(ConnectionHandler onConnectionRequested, MessageHandler onMessageReceived, DisconnectionHandler onDisconnect)
        {
            if (this.IsListening)
            {
                throw new AlreadyListeningException();
            }

            this.OnConnectionRequested = onConnectionRequested;
            this.OnMessageReceived = onMessageReceived;
            this.OnDisconnect = onDisconnect;

            return Task.WhenAll(this.ListeningConnectionAsync(), this.ListeningMessageAsync());
        }

        public async Task BroadcastAsync(string message)
        {
            var currentClients = this.TcpClients.ToList();
            var broadcasting = new List<Task>(currentClients.Count);

            foreach (var remote in currentClients)
            {
                broadcasting.Add(remote.SendAsync(message));
            }

            await Task.WhenAll(broadcasting);
        }

        public async Task CloseConnectionAsync(IRemoteConnection client, string justification)
        {
            if (this.TcpClients.Contains(client))
            {
                var tcpRemote = (client as TcpRemoteConnection);

                await tcpRemote.SendAsync(new Message(NetworkCommand.Disconnected, justification));

                this.RemoveClient(tcpRemote, justification);
            }
        }

        public void StopListening()
        {
            this.Listener.Stop();

            this.ResetConnections();
        }

        #endregion

        #region internal methods

        /// <summary>
        /// Removes the specified client from the list of <see cref="Clients"/>. Also disposes the underlying TcpClient instance.
        /// </summary>
        /// <param name="client">The client to remove</param>
        /// <param name="justification">The justification sent by the client</param>
        internal void RemoveClient(TcpRemoteConnection client, string justification)
        {
            this.TcpClients.Remove(client);

            client.Dispose();

            this.OnDisconnect?.Invoke(client, justification);
        }

        #endregion

        #region private methods

        /// <summary>
        /// Listens for new connection request
        /// </summary>
        private async Task ListeningConnectionAsync()
        {
            this.Listener.Start();

            while (true)
            {
                try
                {
                    var client = await this.Listener.AcceptTcpClientAsync();

                    this.ValidateConnectionAsync(client);
                }
                catch (Exception e)
                {
                    if(!this.IsListening)
                    {
                        // the tcp listener has been stopped, we don't need to throw an exception in that case
                        break;
                    }
                    else
                    {
                        // TODO : log exception
                    }
                }
            }
        }

        /// <summary>
        /// Wait for the connection request, and execute the <see cref="OnConnectionRequested"/> delegate when it arrives
        /// </summary>
        /// <param name="remoteConnection"></param>
        private async Task ValidateConnectionAsync(TcpClient client)
        {
            try
            {
                var remote = new TcpRemoteConnection(client, this);

                Message request = null;

                var chrono = Stopwatch.StartNew();

                while (request == null)
                {
                    if (chrono.ElapsedMilliseconds > this.TcpConfiguration.ConnectionTimeout)
                    {
                        Message response = new Message(NetworkCommand.ConnectionFailed, "The connection request takes to long.");

                        await remote.SendAsync(response);

                        remote.Dispose();

                        return;
                    }

                    request = await remote.ReceiveAsync();

                    if (request == null)
                    {
                        await Task.Delay(this.TcpConfiguration.ListeningTick);
                    }
                }

                if (request.IsValid && request.Command == NetworkCommand.ConnectionRequested)
                {
                    string responseStr = null;

                    var success = this.OnConnectionRequested?.Invoke(remote, request.InnerMessage, out responseStr) ?? true;

                    Message response = new Message(success ? NetworkCommand.ConnectionGranted : NetworkCommand.ConnectionFailed, responseStr);

                    await remote.SendAsync(response);

                    if (success)
                    {
                        this.TcpClients.Add(remote);
                        //log.Debug("Connection Granted");
                    }
                    else
                    {
                        remote.Dispose();
                        //log.Debug("Connection Refused");
                    }
                }
                else
                {
                    Message response = new Message(NetworkCommand.ConnectionFailed, "The connection request is not in the valid format or with the expected command.");

                    await remote.SendAsync(response);
                    remote.Dispose();

                    //log.Debug("Connection Request Invalid");
                }
            }
            catch (Exception e)
            {
                //log.Error(e);
            }
            finally
            {
                //log.DebugFormat("Validation of {0}:{1} finished", remote.IPAddress, remote.Port);
            }
        }

        /// <summary>
        /// Listens for new incomming message and execute the <see cref="OnMessageReceived"/> delegate
        /// </summary>
        private async Task ListeningMessageAsync()
        {
            while (this.IsListening)
            {
                try
                {
                    var disconnectedRemotes = new Dictionary<TcpRemoteConnection, string>();

                    var currentClients = this.TcpClients.ToList();

                    foreach (var remote in currentClients)
                    {
                        for (var message = await remote.ReceiveAsync(); message != null; message = await remote.ReceiveAsync())
                        {
                            if (message.IsValid && message.Command == NetworkCommand.Message)
                            {
                                this.OnMessageReceived?.Invoke(remote, message.InnerMessage);
                            }
                            else if (message.IsValid && message.Command == NetworkCommand.Disconnection)
                            {
                                disconnectedRemotes.Add(remote, message.InnerMessage);
                            }
                        }
                    }

                    foreach (var remote in disconnectedRemotes)
                    {
                        remote.Key.SendAsync(new Message(NetworkCommand.Disconnected, null));

                        this.RemoveClient(remote.Key, remote.Value);
                    }

                    await Task.Delay(this.TcpConfiguration.ListeningTick);
                }
                catch (Exception e)
                {
                    // TODO : log exception
                }
            }

        }

        /// <summary>
        /// Close all the remote connected and clear the list
        /// </summary>
        private void ResetConnections()
        {
            foreach (var remote in this.TcpClients)
            {
                remote.Dispose();
            }

            this.TcpClients.Clear();
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
                    this.StopListening();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
