using System;
using System.Collections.Generic;
using System.Text;

namespace NetworKit.Tcp.Utils
{
    public class TcpConfiguration
    {
        /// <summary>
        /// The encoding type used by both <see cref="TcpNetworkServer"/> and <see cref="TcpNetworkClient"/>.
        /// </summary>
        public Encoding UsedEncoding { get; set; } = Encoding.Unicode;

        /// <summary>
        /// The time (in ms) before the connection request failed and timeout.
        /// </summary>
        public int ConnectionTimeout { get; set; } = 10000;

        /// <summary>
        /// The time (in ms) between two messages lookup, for both <see cref="TcpNetworkServer"/> and <see cref="TcpNetworkClient"/>.
        /// </summary>
        public int ListeningTick { get; set; } = 100;

        /// <summary>
        /// The sequence of characters isolating two messages within the Tcp buffer.
        /// </summary>
        public string MessageBound { get; set; } = "><";
    }
}
