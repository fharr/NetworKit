namespace NetworKit.Tcp
{
    using NetworKit.Exceptions;
    using NetworKit.Tcp.Utils;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;

    public class TcpNetworkClient : INetworkClient
    {
        #region public properties

        public int Port { get { return this.Server?.Port ?? -1; } }

        public bool IsConnected { get { return this.Server?.IsConnected ?? false; } }

        public TcpConfiguration TcpConfiguration { get; }

        #endregion

        #region private properties

        /// <summary>
        /// The server on which this instance is connected.
        /// </summary>
        private TcpRemoteConnection Server { get; set; }

        /// <summary>
        /// The deletage executed when a new message is received.
        /// </summary>
        private MessageHandler OnMessageReceived { get; set; }

        #endregion

        #region constructors

        public TcpNetworkClient()
        {
            this.TcpConfiguration = new TcpConfiguration();
        }

        #endregion

        #region public methods

        public async Task<string> ConnectAsync(string ipAddress, int port, MessageHandler onMessageReceived, string message)
        {
            if (this.IsConnected)
            {
                throw new AlreadyConnectedException();
            }

            var server = new TcpClient();

            try
            {
                await server.ConnectAsync(ipAddress, port);
            }
            catch (SocketException e)
            {
                // TODO : log exception

                server.Dispose();

                throw new ConnectionFailedException(ConnectionFailedType.RemoteConnectionUnreachable);
            }

            this.Server = new TcpRemoteConnection(server, this);

            this.OnMessageReceived = onMessageReceived;

            var connectionRequest = new Message(NetworkCommand.ConnectionRequested, message);

            await this.Server.SendAsync(connectionRequest);

            var connectionResponse = await this.ReceiveMessageAsync(this.TcpConfiguration.ConnectionTimeout);

            if (connectionResponse == null)
            {
                this.ResetConnection();
                throw new ConnectionFailedException(ConnectionFailedType.Timeout);
            }
            else if (!connectionResponse.IsValid)
            {
                this.ResetConnection();
                throw new ConnectionFailedException(ConnectionFailedType.InvalidResponse);
            }
            else if (connectionResponse.Command == NetworkCommand.ConnectionFailed)
            {
                this.ResetConnection();
                throw new ConnectionFailedException(ConnectionFailedType.ConnectionRefused);
            }
            else if (connectionResponse.Command != NetworkCommand.ConnectionGranted)
            {
                this.ResetConnection();
                throw new ConnectionFailedException(ConnectionFailedType.Other);
            }

            this.ListeningAsync();

            return connectionResponse.InnerMessage;
        }

        public Task SendAsync(string message)
        {
            if (!this.IsConnected)
            {
                throw new NotConnectedException();
            }

            return this.Server.SendAsync(message);
        }

        public async Task DisconnectAsync(string justification)
        {
            if (!this.IsConnected)
            {
                throw new NotConnectedException();
            }

            var disconnection = new Message(NetworkCommand.Disconnection, justification);

            await this.Server.SendAsync(disconnection);

            var timer = Stopwatch.StartNew();

            while (timer.ElapsedMilliseconds < this.TcpConfiguration.DisconnectionTimeout)
            {
                var message = await this.ReceiveMessageAsync(this.TcpConfiguration.ListeningTick);

                if (message != null && message.IsValid && message.Command == NetworkCommand.Disconnected)
                {
                    // TODO : use message.InnerMessage
                    break;
                }
            }

            this.ResetConnection();
        }

        #endregion

        #region internal methods

        internal void ResetConnection()
        {
            this.Server?.Dispose();

            this.Server = null;
            this.OnMessageReceived = null;
        }

        #endregion

        #region private methods

        private async Task ListeningAsync()
        {
            // TODO : try/catch to manage disconnection
            while (this.IsConnected)
            {
                var message = await this.Server.ReceiveAsync();

                if (message == null)
                {
                    await Task.Delay(this.TcpConfiguration.ListeningTick);
                }
                else
                {
                    if (message.IsValid && message.Command == NetworkCommand.Message)
                    {
                        this.OnMessageReceived?.Invoke(this.Server, message.InnerMessage);
                    }
                    else if (message.IsValid && message.Command == NetworkCommand.Disconnected)
                    {
                        this.ResetConnection();
                        throw new Exception("TODO");
                    }
                    else
                    {
                        // log warning : unexpected message received
                    }
                }
            }
        }

        private async Task<Message> ReceiveMessageAsync(long timeout)
        {
            var timer = Stopwatch.StartNew();

            while (true)
            {
                var response = await this.Server.ReceiveAsync();

                if (response == null && timer.ElapsedMilliseconds > timeout)
                {
                    return null;
                }
                else if (response == null)
                {
                    await Task.Delay(this.TcpConfiguration.ListeningTick);
                }
                else
                {
                    return response;
                }
            }
        }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false;

        protected virtual async void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing && this.IsConnected)
                {
                    await this.DisconnectAsync(null);
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