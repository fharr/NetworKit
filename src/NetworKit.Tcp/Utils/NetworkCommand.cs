namespace NetworKit.Tcp.Utils
{
    internal enum NetworkCommand
    {
        ConnectionRequested,
        ConnectionGranted,
        ConnectionFailed,
        Message,
        Disconnection,
        Disconnected
    }
}
