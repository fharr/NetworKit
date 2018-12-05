namespace NetworKit.Tcp
{
    using System.Text;

    public abstract class TcpNetworkSettings : INetworkSettings
    {
        public int LocalPort { get; set; } = 1337;
        public int ConnectionTimeout { get; set; } = 10000;

        public int ListeningTick { get; set; } = 100;
        public Encoding MessageEncoding { get; set; } = Encoding.UTF32;
        public string MessageSeparator { get; set; } = "<SEP>";
    }
}
