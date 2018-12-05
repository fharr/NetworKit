namespace NetworKit.Exceptions
{
    using System;

    public class ConnectionLostException : Exception
    {
        #region constructors

        /// <summary>
        /// Instantiates a new connection lost exception.
        /// </summary>
        public ConnectionLostException(Exception cause)
            : base("The connection with the server has been lost.", cause)
        { }

        /// <summary>
        /// Instantiates a new connection lost exception.
        /// </summary>
        public ConnectionLostException(string cause)
            : base(cause)
        { }

        #endregion
    }
}
