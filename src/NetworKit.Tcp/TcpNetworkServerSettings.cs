namespace NetworKit.Tcp
{
    public class TcpNetworkServerSettings : TcpNetworkSettings, INetworkServerSettings
    {
        public INetworkServerMessageHandler Handler { internal get; set; }
    }
}
