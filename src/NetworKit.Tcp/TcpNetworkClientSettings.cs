namespace NetworKit.Tcp
{
    public class TcpNetworkClientSettings : TcpNetworkSettings, INetworkClientSettings
    {
        public INetworkClientMessageHandler Handler { internal get; set; }
    }
}
