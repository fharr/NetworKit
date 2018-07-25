namespace NetworKit.Tcp
{
    using NetworKit.Exceptions;
    using NetworKit.Tcp.Utils;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;

    internal class TcpRemoteConnection : IRemoteConnection
    {
        #region properties

        public string IPAddress { get { return (this.TcpClient.Client.RemoteEndPoint as IPEndPoint).Address.ToString(); } }
        public int Port { get { return (this.TcpClient.Client.RemoteEndPoint as IPEndPoint).Port; } }
        public long Ping
        {
            get
            {
                return this.PingUtil.Send((this.TcpClient.Client.RemoteEndPoint as IPEndPoint).Address).RoundtripTime;
            }
        }

        /// <summary>
        /// The tcp client used to send and receive message
        /// </summary>
        internal TcpClient TcpClient { get; set; }

        #endregion

        #region private properties

        /// <summary>
        /// The buffer used to store partial messages received
        /// </summary>
        private StringBuilder Buffer { get; set; }

        /// <summary>
        /// The ping instance used to get the ping between two remote connection
        /// </summary>
        private Ping PingUtil { get; set; }

        /// <summary>
        /// The TcpNetworkServer which is managin this instance of TcpRemoteConnection
        /// </summary>
        private TcpNetworkServer Server { get; set; }

        /// <summary>
        /// The TcpNetworkClient which is managing this instance of TcpRemoteConnection
        /// </summary>
        private TcpNetworkClient Client { get; set; }

        /// <summary>
        /// The Encoding used to serialize messages
        /// </summary>
        private Encoding Encoder { get; set; }

        #endregion

        #region constructors

        /// <summary>
        ///Instantiates a new tcp remote connection
        /// </summary>
        /// <param name="client">The TcpClient used to send and receive message</param>
        /// <param name="separator">The separator used to distinguish two different message</param>
        internal TcpRemoteConnection(TcpClient client)
        {
            this.TcpClient = client;
            this.Buffer = new StringBuilder();

            this.Encoder = Encoding.GetEncoding(Settings.Default.Encoding);

            this.PingUtil = new Ping();
        }

        /// <summary>
        /// Instantiates a new tcp remote connection
        /// </summary>
        /// <param name="client">The TcpClient used to send and receive message</param>
        /// <param name="separator">The separator used to distinguish two different message</param>
        /// <param name="networkServer">The server managing the new instance</param>
        public TcpRemoteConnection(TcpClient client, TcpNetworkServer networkServer)
            : this(client)
        {
            this.Server = networkServer;
        }

        /// <summary>
        /// Instantiates a new tcp remote connection
        /// </summary>
        /// <param name="client">The TcpClient used to send and receive message</param>
        /// <param name="separator">The separator used to distinguish two different message</param>
        /// <param name="socketClient">The client managing the new instance</param>
        public TcpRemoteConnection(TcpClient client, TcpNetworkClient networkClient)
            : this(client)
        {
            this.Client = networkClient;
        }

        #endregion

        #region methods

        public Task SendAsync(string message)
        {
            var msg = new Message(NetworkCommand.Message, message);

            return this.Send(msg);
        }

        #endregion

        #region internal methods

        /// <summary>
        /// Sends the specified message to the remote connection
        /// </summary>
        /// <param name="message"></param>
        internal async Task SendAsync(Message message)
        {
            var datagram = this.SerializeMessage(message.ToString() + Settings.Default.MessageBound);

            try
            {
                await this.TcpClient.GetStream().WriteAsync(datagram, 0, datagram.Length);
            }
            catch (SocketException e)
            {
                switch (e.ErrorCode)
                {
                    case 10053:
                        // The remote connection is disconnected
                        if (this.Server != null)
                        {
                            this.Server.Clients.Remove(this);
                            this.Server.OnDisconnect?.Invoke(this, null);
                        }
                        else if (this.Client != null)
                        {
                            this.Client.ResetConnection();
                            throw new ConnectionLostException(e);
                        }
                        break;
                    default:
                        // In all the other cases, the exception is thrown
                        throw e;
                }
            }
        }

        /// <summary>
        /// Looks into the TcpClient for a complete message and returns it. null if the message is incomplete.
        /// </summary>
        /// <returns>The received message. Null if not complete</returns>
        internal async Task<Message> ReceiveAsync()
        {
            // Retrieving latest bytes
            if (this.TcpClient.Available > 0)
            {
                byte[] bytes = new byte[this.TcpClient.Available];

                await this.TcpClient.GetStream().ReadAsync(bytes, 0, bytes.Length);

                string addition = this.DeserializeDatagram(bytes);

                this.Buffer.Append(addition);
            }

            Message message = null;

            var separator = Settings.Default.MessageBound;

            var currentBuffer = this.Buffer.ToString();
            var i = currentBuffer.IndexOf(separator);

            // Extracting next message
            if (i != -1)
            {
                message = Message.Parse(currentBuffer.Substring(0, i));

                this.Buffer.Remove(0, i + separator.Lenght);
            }

            return message;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Serializes a string into a byte array
        /// </summary>
        /// <param name="message">The string to serialize</param>
        /// <returns>The string serialized into a byte array</returns>
        private byte[] SerializeMessage(string message)
        {
            var datagram = this.Encoder.GetBytes(message);

            return datagram;
        }

        /// <summary>
        /// Deserializes a byte array into a string
        /// </summary>
        /// <param name="datagram">The byte array to deserialize</param>
        /// <returns>The string deserialized</returns>
        private string DeserializeDatagram(byte[] datagram)
        {
            var message = this.Encoder.GetString(datagram);

            return message;
        }

        #endregion

        #region overridden methods

        public override bool Equals(object obj)
        {
            if (!(obj is TcpRemoteConnection other))
                return false;

            return other.IPAddress == this.IPAddress && other.Port == this.Port;
        }

        public override int GetHashCode()
        {
            return this.TcpClient.GetHashCode();
        }

        #endregion
    }
}
