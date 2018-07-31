namespace NetworKit.Tcp.Utils
{
    using System;

    internal class Message
    {
        #region properties

        public NetworkCommand Command { get; }

        public string InnerMessage { get; }

        public bool IsValid { get; }

        #endregion

        #region constructors

        private Message(string message)
        {
            var i = message.IndexOf('|');

            this.IsValid = i != -1;

            if (this.IsValid)
            {
                this.Command = (NetworkCommand)Enum.Parse(typeof(NetworkCommand), message.Substring(0, i));
                this.InnerMessage = message.Substring(i+1);
            }
        }

        public Message(NetworkCommand command, string message)
        {
            this.Command = command;
            this.InnerMessage = message;
            this.IsValid = true;
        }

        #endregion

        #region methods

        public override string ToString()
        {
            return String.Format("{0}|{1}", this.Command, this.InnerMessage);
        }

        #endregion

        #region static methods

        public static Message Parse(string message)
        {
            return new Message(message);
        }

        #endregion
    }
}
