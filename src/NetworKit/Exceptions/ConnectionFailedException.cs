namespace NetworKit.Exceptions
{
    using System;
    using System.ComponentModel;

    public class ConnectionFailedException : Exception
    {
        #region properties

        /// <summary>
        /// The cause of the connection failure
        /// </summary>
        public ConnectionFailedType TypeErreur { get; }

        #endregion

        #region constructors

        /// <summary>
        /// Instantiates a new connection failed exception, giving the cause in parameters. If the message is not specified, the default one will be provided.
        /// </summary>
        public ConnectionFailedException(ConnectionFailedType cause, string message = null)
            : base(message ?? GetErrorMessage(cause))
        {
            this.TypeErreur = cause;
        }

        /// <summary>
        /// Instantiates a new connection failed exception, giving the cause in parameters and the associated inner exception. If the message is not specified, the default one will be provided.
        /// </summary>
        /// <param name="innerException">The exception that cause the connection failure</param>
        /// <param name="cause">The cause of the connection failure</param>
        public ConnectionFailedException(ConnectionFailedType cause, Exception innerException, string message = null)
            : base(message ?? GetErrorMessage(cause), innerException)
        {
            this.TypeErreur = cause;
        }

        #endregion

        #region static functions

        private static string GetErrorMessage(ConnectionFailedType type)
        {
            var memInfo = typeof(ConnectionFailedType).GetMember(type.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

            return ((DescriptionAttribute)attributes[0]).Description;
        }

        #endregion
    }

    public enum ConnectionFailedType
    {
        [Description("Your connection requestion timeout.")]
        Timeout,
        [Description("The connection has been refused by the server.")]
        ConnectionRefused,
        [Description("The specified remote connection is not reachable.")]
        RemoteConnectionUnreachable,
        [Description("The response from the server was not at the expected format.")]
        InvalidResponse,
        [Description("An unexpected connection response has been received.")]
        Other
    }
}
