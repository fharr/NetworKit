namespace NetworKit
{
    public interface INetworkServerSettings : INetworkSettings
    {
        INetworkServerMessageHandler Handler { set; }
    }
}
