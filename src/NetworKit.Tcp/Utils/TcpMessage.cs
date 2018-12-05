namespace NetworKit.Tcp.Utils
{
    using System;

    internal class TcpMessage
    {
        #region properties

        public TcpNetworkCommand Command { get; }
        public string InnerMessage { get; }
        public bool IsValid { get; }

        #endregion

        #region constructors

        public TcpMessage(TcpNetworkCommand command, string message = null)
        {
            this.IsValid = true;
            this.Command = command;
            this.InnerMessage = message;
        }

        private TcpMessage(string message)
        {
            var index = message.IndexOf('|');

            if (index == -1)
            {
                this.IsValid = false;
                this.Command = TcpNetworkCommand.None;
                this.InnerMessage = message;

                return;
            }

            var cmd = message.Substring(0, index);
            var msg = message.Substring(index + 1);

            TcpNetworkCommand command;
            if (Enum.TryParse(cmd, out command) && Enum.IsDefined(typeof(TcpNetworkCommand), command))
            {
                this.IsValid = true;
                this.InnerMessage = msg;
                this.Command = command;
            }
            else
            {
                this.IsValid = false;
                this.InnerMessage = message;
                this.Command = TcpNetworkCommand.None;
            }
        }

        #endregion

        #region methods

        public override string ToString()
        {
            return $"{(int)this.Command}|{this.InnerMessage}";
        }

        #endregion

        #region static methods

        public static TcpMessage Parse(string message)
        {
            return new TcpMessage(message);
        }

        #endregion
    }
}
