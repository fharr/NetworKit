namespace NetworKit
{
    using System;
    using System.Threading.Tasks;

    public interface INetworkClient : IDisposable
    {
        #region properties

        INetworkClientSettings Settings { get; }

        /// <summary>
        /// Gets a value that indicates whether the client is already connected to a server.
        /// </summary>
        bool IsConnected { get; }

        #endregion

        #region methods 

        /// <summary>
        /// Connects to the specified remote server.
        /// </summary>
        /// <param name="ipAddress">the IP address of the server to connect with</param>
        /// <param name="port">the listening port of the server to connect on</param>
        /// <param name="onMessageReceived">the delegate that is executed when a message is sended by the server to this client</param>
        /// <param name="message">an additionnal message to the request</param>
        /// <exception cref="Exceptions.ConnectionFailedException">When the SocketClient can't connect to the remote connection.</exception>
        /// <returns>the response of the server</returns>
        Task<string> ConnectAsync(string serverAddress, int serverPort, string request = null);

        /// <summary>
        /// Sends a message to the connected server
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <exception cref="Exceptions.NotConnectedException">When the client is not connected</exception>
        /// <exception cref="Exceptions.ConnectionLostException">When the server is no longer available</exception>
        Task SendAsync(string message);

        /// <summary>
        /// Shutdowns the connection with the remote server
        /// </summary>
        /// <param name="justification">the justification to send to the server</param>
        Task DisconnectAsync(string justification = null);

        #endregion
    }
}
