using System;
using System.Collections.Generic;
using System.Text;

namespace NetworKit.Tcp.Exceptions
{
    public class MessageBoundCollisionException : Exception
    {
        #region properties

        public string TcpMessage { get; }
        public string Separator { get; }

        #endregion

        #region constructors

        public MessageBoundCollisionException(string message, string separator)
            : base("Your message contains the separator used internally to differentiate messages within the TCP buffer. Do not use these characters in your message or overwrite the MessageBound property of both your client and your server instances.")
        {
            this.TcpMessage = message;
            this.Separator = separator;
        }

        #endregion
    }
}
