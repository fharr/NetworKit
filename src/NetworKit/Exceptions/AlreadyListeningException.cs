namespace NetworKit.Exceptions
{
    using System;

    public class AlreadyListeningException : Exception
    {
        #region constructors

        /// <summary>
        /// Instantiates a new already listening exception.
        /// </summary>
        public AlreadyListeningException()
            : base("This server is already listening.")
        { }

        #endregion
    }
}
