namespace NetworKit
{
    public interface INetworkClientMessageHandler
    {
        void OnMessageReceived(string message);
        void OnServerDisconnection(string justfication);
    }
}
