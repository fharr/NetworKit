namespace NetworKit.Tcp
{
    using NetworKit.Exceptions;
    using NetworKit.Tcp.Utils;
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;

    public class TcpNetworkClient : INetworkClient
    {
        #region public properties

        public int Port { get { return this.Server.Port; } }

        public bool IsConnected { get { return this.Server.IsConnected; } }

        public TcpConfiguration TcpConfiguration { get; }

        #endregion

        #region private properties

        /// <summary>
        /// The server on which this instance is connected.
        /// </summary>
        private TcpRemoteConnection Server { get; }

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

        public async Task<string> ConnectAsync(string ipAddress, int port, MessageHandler onMessageReceived, string message)
        {
            if (this.IsConnected)
            {
                throw new AlreadyConnectedException();
            }

            this.OnMessageReceived = onMessageReceived;

            try
            {
                await this.Server.
            }
        }

        public Task DisconnectAsync(string justification)
        {
            throw new NotImplementedException();
        }

        public Task SendAsync(string message)
        {
            throw new NotImplementedException();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TcpNetworkClient() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        internal void ResetConnection()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
