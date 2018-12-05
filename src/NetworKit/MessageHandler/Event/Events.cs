namespace NetworKit.MessageHandler.Event
{
    public delegate void ClientMessageHandler(string message);
    public delegate void ServerMessageHandler(IRemoteConnection sender, string message);
}
