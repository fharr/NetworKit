namespace NetworKit
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface INetworkServer : IDisposable
    {
        #region properties

        /// <summary>
        /// The port on which the server is listening.
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Gets a value that indicates whether the server is listening.
        /// </summary>
        bool IsListening { get; }

        /// <summary>
        /// Gets a list that indicates all the clients connected to the server.
        /// </summary>
        IEnumerable<IRemoteConnection> Clients { get; }

        #endregion

        #region methods

        /// <summary>
        /// Starts listening for messages on the <see cref="Port"/>.
        /// </summary>
        /// <param name="onConnectionRequested">the delegate that is executed when a connection request arrives</param>
        /// <param name="onMessageReceived">the delegate that is executed when a message from a connected client arrives</param>
        /// <param name="onDisconnect">the delegate that is executed when a client is disconnected</param>
        /// <exception cref="Exceptions.AlreadyListeningException">If the socket is already listening</exception>
        Task StartListening(ConnectionHandler onConnectionRequested, MessageHandler onMessageReceived, DisconnectionHandler onDisconnect);

        /// <summary>
        /// Broadcasts the specified message to all the connected clients.
        /// </summary>
        /// <param name="message">The message to broadcast</param>
        Task Broadcast(string message);

        /// <summary>
        /// Closes the connection with the specified client.
        /// </summary>
        /// <param name="client">the client to disconnect with</param>
        /// <param name="justification">the justfication of the disconnection</param>
        Task CloseConnection(IRemoteConnection client, string justification);

        /// <summary>
        /// Stops the server from listening.
        /// </summary>
        void StopListening();

        #endregion
    }
}
