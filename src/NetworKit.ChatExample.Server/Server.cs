namespace NetworKit.ChatExample.Server
{
    using NetworKit.MessageHandler.Delegate;
    using NetworKit.Tcp;
    using System;

    class Server
    {
        static INetworkServer NetworkServer;

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("================================");
                Console.WriteLine("======= NetworKit Example ======");
                Console.WriteLine("========== Chat Server =========");
                Console.WriteLine("================================");

                Console.WriteLine();

                Console.Write("Select the port on which you want to set up your server: ");
                int port;
                string portStr = null;
                do
                {
                    if (!String.IsNullOrWhiteSpace(portStr))
                    {
                        Console.Write("Wrong format. Please enter a valid integer: ");
                    }

                    portStr = Console.ReadLine();
                } while (!int.TryParse(portStr, out port));

                Console.WriteLine();

                using (NetworkServer = new TcpNetworkServer(new NetworkServerDelegateMessageHandler(ConnectionRequested, ServerMessageReceived, ServerDisconnect)))
                {
                    NetworkServer.Settings.LocalPort = port;

                    NetworkServer.StartListening((remote,request) => new ConnectionStatus(true, $"Welcome {remote.IPAddress}:{remote.Port}"));

                    Console.WriteLine("Server started! Press Q to stop the server.");
                    Console.WriteLine();

                    ConsoleKeyInfo exit;
                    do
                    {
                        exit = Console.ReadKey(true);
                    } while (exit.Key != ConsoleKey.Q);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"An unexpected error occured: {e.Message}");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
            }
            finally
            {
                Console.WriteLine("================================");
                Console.WriteLine("======= NetworKit Example ======");
                Console.WriteLine("========== Chat Server =========");
                Console.WriteLine("================================");

                Console.WriteLine();

                Console.WriteLine("press a key to finish...");
                Console.ReadKey();
            }
        }

        #region delegates

        private static async void ConnectionRequested(IRemoteConnection sender, string request)
        {
            var message = $"New connection from {sender.IPAddress}:{sender.Port} => {request}";

            Console.WriteLine(message);

            await NetworkServer.BroadcastAsync(message);
        }

        private static async void ServerMessageReceived(IRemoteConnection sender, string message)
        {
            Console.WriteLine($"Message received from {sender.IPAddress}:{sender.Port}: {message}");

            await NetworkServer.BroadcastAsync($"{sender.IPAddress}:{sender.Port} says: {message}");
        }

        private async static void ServerDisconnect(IRemoteConnection remote, string justification)
        {
            var message = $"{remote.IPAddress}:{remote.Port} is now disconnected ({justification})";

            Console.WriteLine(message);

            await NetworkServer.BroadcastAsync(message);
        }

        #endregion
    }
}