namespace NetworKit
{
    using System;
    using System.Threading.Tasks;

    public interface IRemoteConnection : IDisposable
    {
        #region properties

        /// <summary>
        /// Gets a value that indicates whether the remote connection is online.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// The IP address of the remote connection.
        /// </summary>
        string IPAddress { get; }

        /// <summary>
        /// The port used by the remote connection.
        /// </summary>
        int Port { get; }

        /// <summary>
        /// The ping between this instance and the remote connection.
        /// </summary>
        long Ping { get; }

        #endregion

        #region methods

        /// <summary>
        /// Send a message to the remote connection.
        /// </summary>
        /// <param name="message">the message to send</param>
        Task SendAsync(string message);

        #endregion
    }
}
