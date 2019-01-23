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
        public ConnectionFailedType ErrorType { get; }

        public string ServerResponse { get; }

        #endregion

        #region constructors

        /// <summary>
        /// Instantiates a new connection failed exception, giving the cause in parameters.
        /// </summary>
        /// <param name="cause">The cause of the connection failure</param>
        /// <param name="response">The actual server response</param>
        public ConnectionFailedException(ConnectionFailedType cause, string response = null)
            : base(GetErrorMessage(cause))
        {
            this.ErrorType = cause;
            this.ServerResponse = response;
        }

        /// <summary>
        /// Instantiates a new connection failed exception, giving the cause in parameters and the associated inner exception.
        /// </summary>
        /// <param name="cause">The cause of the connection failure</param>
        /// <param name="innerException">The exception that cause the connection failure</param>
        /// <param name="response">The actual server response</param>
        public ConnectionFailedException(ConnectionFailedType cause, Exception innerException, string response = null)
            : base(GetErrorMessage(cause), innerException)
        {
            this.ErrorType = cause;
            this.ServerResponse = response;
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
        [Description("The specified remote connection is not reachable.")]
        RemoteConnectionUnreachable,
        [Description("The connection request failed.")]
        ConnectionRequestFailed,
        [Description("Your connection request timeout.")]
        ConnectionTimeout,
        [Description("The connection has been refused by the server.")]
        ConnectionRefused,
        [Description("The response from the server was not at the expected format.")]
        InvalidResponse,
        [Description("An unexpected connection response has been received.")]
        UnexpectedResponse,
        [Description("An unexpected error occured. See the inner exception.")]
        Other
    }
}
