namespace NetworKit
{
    using System.Threading.Tasks;

    public interface IRemoteConnection
    {
        #region properties

        /// <summary>
        /// The IP address of the remote connection
        /// </summary>
        string IPAddress { get; }

        /// <summary>
        /// The port used by the remote connection
        /// </summary>
        int Port { get; }

        /// <summary>
        /// The ping between this instance and the remote connection
        /// </summary>
        long Ping { get; }

        #endregion

        #region methods

        /// <summary>
        /// Send a message to the remote connection
        /// </summary>
        /// <param name="message">the message to send</param>
        Task SendAsync(string message);

        #endregion
    }
}
