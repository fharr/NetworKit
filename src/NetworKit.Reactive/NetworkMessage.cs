namespace NetworKit.Reactive
{
    public class NetworkMessage
    {
        #region properties

        public IRemoteConnection RemoteConnection { get; }
        public string Message { get; }

        #endregion

        #region constructors

        internal NetworkMessage(IRemoteConnection remoteConnection, string message = null)
        {
            this.RemoteConnection = remoteConnection;
            this.Message = message;
        }

        #endregion
    }
}
