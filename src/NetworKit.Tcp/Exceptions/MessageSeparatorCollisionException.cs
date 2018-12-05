namespace NetworKit.Tcp.Exceptions
{
    using System;

    public class MessageSeparatorCollisionException : Exception
    {
        #region properties

        public string TcpMessage { get; }
        public string Separator { get; }

        #endregion

        #region constructors

        public MessageSeparatorCollisionException(string message, string separator)
            : base("Your message contains the separator used internally to differentiate messages within the TCP buffer. Do not use these characters in your message or overwrite the MessageBound property of both your client and your server instances.")
        {
            this.TcpMessage = message;
            this.Separator = separator;
        }

        #endregion
    }
}
