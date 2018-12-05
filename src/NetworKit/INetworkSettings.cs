namespace NetworKit
{
    public interface INetworkSettings
    {
        int LocalPort { get; set; }
        int ConnectionTimeout { get; set; }
    }
}
