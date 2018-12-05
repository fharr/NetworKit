namespace NetworKit
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface INetworkServer : IDisposable
    {
        #region properties

        /// <summary>
        /// Gets a value that indicates whether the server is listening.
        /// </summary>
        bool IsListening { get; }

        INetworkServerSettings Settings { get; }

        /// <summary>
        /// Gets a list that indicates all the clients connected to the server.
        /// </summary>
        IReadOnlyCollection<IRemoteConnection> Clients { get; }

        #endregion

        #region methods

        /// <summary>
        /// Starts listening for messages on the <see cref="Port"/>.
        /// </summary>
        /// <param name="onConnectionRequested">the delegate that is executed when a connection request arrives</param>
        /// <param name="onMessageReceived">the delegate that is executed when a message from a connected client arrives</param>
        /// <param name="onDisconnect">the delegate that is executed when a client is disconnected</param>
        /// <exception cref="Exceptions.AlreadyListeningException">If the socket is already listening</exception>
        void StartListening(Func<IRemoteConnection, string, ConnectionStatus> validateConnection = null);

        /// <summary>
        /// Broadcasts the specified message to all the connected clients.
        /// </summary>
        /// <param name="message">The message to broadcast</param>
        Task BroadcastAsync(string message);

        Task SendToAsync(IRemoteConnection remote, string message);

        /// <summary>
        /// Closes the connection with the specified client.
        /// </summary>
        /// <param name="client">the client to disconnect with</param>
        /// <param name="justification">the justfication of the disconnection</param>
        Task CloseConnectionAsync(IRemoteConnection client, string justification = null);

        /// <summary>
        /// Stops the server from listening.
        /// </summary>
        Task StopListening();

        #endregion
    }
}
