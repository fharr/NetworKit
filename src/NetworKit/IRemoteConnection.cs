namespace NetworKit
{
    public interface IRemoteConnection
    {
        /// <summary>
        /// Gets a value that indicates whether the remote connection is online.
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// The IP address of the remote connection.
        /// </summary>
        string IPAddress { get; }

        /// <summary>
        /// The port used by the remote connection.
        /// </summary>
        int Port { get; }
    }
}
