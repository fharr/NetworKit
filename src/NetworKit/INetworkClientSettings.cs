namespace NetworKit
{
    public interface INetworkClientSettings : INetworkSettings
    {
        INetworkClientMessageHandler Handler { set; }
    }
}
