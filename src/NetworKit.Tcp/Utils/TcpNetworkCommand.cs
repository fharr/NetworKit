namespace NetworKit.Tcp.Utils
{
    internal enum TcpNetworkCommand
    {
        None,
        ConnectionRequest,
        ConnectionGranted,
        ConnectionDenied,
        Message,
        Disconnection
    }
}
