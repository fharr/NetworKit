namespace NetworKit.Exceptions
{
    using System;

    public class AlreadyConnectedException : Exception
    {
        #region constructors

        public AlreadyConnectedException() : base("This instance is already connected to a server. Please close the connection before connecting to another.")
        { }

        #endregion
    }
}
