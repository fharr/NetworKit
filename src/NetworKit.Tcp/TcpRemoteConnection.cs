namespace NetworKit.Tcp
{
    using NetworKit.Exceptions;
    using NetworKit.Tcp.Exceptions;
    using NetworKit.Tcp.Utils;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;

    internal class TcpRemoteConnection : IRemoteConnection, IDisposable
    {
        #region fields

        private readonly TcpClient _client;
        private readonly TcpNetworkSettings _config;
        private readonly StringBuilder _messageBuffer;

        #endregion

        #region properties

        public bool Connected
        {
            get
            {
                this.EnsureIsAlive();
                return _client.Connected;
            }
        }
        public string IPAddress
        {
            get
            {
                this.EnsureIsAliveAndConnected();
                return (_client.Client.RemoteEndPoint as IPEndPoint).Address.ToString();
            }
        }
        public int Port
        {
            get
            {
                this.EnsureIsAliveAndConnected();
                return (_client.Client.RemoteEndPoint as IPEndPoint).Port;
            }
        }

        #endregion

        #region constructors

        internal TcpRemoteConnection(TcpClient client, TcpNetworkSettings config)
        {
            _client = client;
            _config = config;
            _messageBuffer = new StringBuilder();

            this.EnsureIsAliveAndConnected();
        }

        #endregion

        #region internal methods

        internal async Task SendAsync(TcpMessage message)
        {
            this.EnsureIsAliveAndConnected();

            if (message.InnerMessage?.Contains(_config.MessageSeparator) ?? false)
            {
                throw new MessageSeparatorCollisionException(message.InnerMessage, _config.MessageSeparator);
            }

            var msg = message.ToString() + _config.MessageSeparator;

            var bytes = _config.MessageEncoding.GetBytes(msg);

            try
            {
                await _client.GetStream().WriteAsync(bytes, 0, bytes.Length);
            }
            catch (IOException e)
            {
                throw new ConnectionLostException(e);
            }
        }

        internal async Task<TcpMessage> ReceiveAsync()
        {
            this.EnsureIsAliveAndConnected();

            if (_client.Available > 0)
            {
                var buffer = new byte[_client.Available];
                await _client.GetStream().ReadAsync(buffer, 0, buffer.Length);

                var data = _config.MessageEncoding.GetString(buffer);
                _messageBuffer.Append(data);
            }

            var messages = _messageBuffer.ToString();
            var index = messages.IndexOf(_config.MessageSeparator);

            if (index == -1)
            {
                return null;
            }

            _messageBuffer.Remove(0, index + _config.MessageSeparator.Length);

            return TcpMessage.Parse(messages.Substring(0, index));
        }

        internal async Task<TcpMessage> ReceiveAsync(int timeout)
        {
            var time = Stopwatch.StartNew();
            while (time.ElapsedMilliseconds < timeout)
            {
                await Task.Delay(_config.ListeningTick);

                var response = await this.ReceiveAsync();

                if (response == null)
                    continue;

                return response;
            }

            return null;
        }

        #endregion

        #region overridden methods

        public override bool Equals(object obj)
        {
            var other = obj as TcpRemoteConnection;
            if (other == null)
                return false;

            return other.IPAddress == this.IPAddress && other.Port == this.Port;

            //if (!(obj is TcpRemoteConnection other))
            //    return false;

            //return other.IPAddress == this.IPAddress && other.Port == this.Port;
        }

        public override int GetHashCode()
        {
            return _client.GetHashCode();
        }

        #endregion

        #region private methods

        private void EnsureIsAlive()
        {
            if (_disposedValue)
            {
                throw new ObjectDisposedException(nameof(TcpRemoteConnection));
            }
        }

        private void EnsureIsAliveAndConnected()
        {
            this.EnsureIsAlive();

            if (!_client.Connected)
            {
                throw new NotConnectedException();
            }
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
                    _client.Close();
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
