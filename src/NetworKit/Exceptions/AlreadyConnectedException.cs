using System;
using System.Collections.Generic;
using System.Text;

namespace NetworKit.Exceptions
{
    public class AlreadyConnectedException : Exception
    {
        #region constructors

        public AlreadyConnectedException() : base("This instance is already connected to a server. Please dispose it before connecting to another socket.")
        { }

        #endregion
    }
}
