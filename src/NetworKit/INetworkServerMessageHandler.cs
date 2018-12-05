namespace NetworKit
{
    public interface INetworkServerMessageHandler
    {
        void OnNewConnection(IRemoteConnection client, string connectionRequest);
        void OnMessageReceived(IRemoteConnection client, string message);
        void OnClientDisconnection(IRemoteConnection client, string justification);
    }
}
