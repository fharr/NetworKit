namespace NetworKit.ChatExample.Client
{
    using NetworKit.MessageHandler.Delegate;
    using NetworKit.Tcp;
    using System;

    class Client
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("================================");
                Console.WriteLine("======= NetworKit Example ======");
                Console.WriteLine("========== Chat Client =========");
                Console.WriteLine("================================");

                Console.WriteLine();

                Console.Write("Select the port of your chat server: ");
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

                using (var networkClient = new TcpNetworkClient(new NetworkClientDelegateMessageHandler(MessageReceived, ServerDisconnectedReceived)))
                {
                    networkClient.ConnectAsync("127.0.0.1", port, "Hello !").GetAwaiter().GetResult();

                    Console.WriteLine("Connection established. Press Q to stop the client.");
                    Console.WriteLine();

                    while (true)
                    {
                        var input = Console.ReadLine();

                        if (input.ToUpper() == "Q")
                        {
                            break;
                        }
                        else
                        {
                            networkClient.SendAsync(input).GetAwaiter().GetResult();
                        }
                    }
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
                Console.WriteLine("========== Chat Client =========");
                Console.WriteLine("================================");

                Console.WriteLine();

                Console.WriteLine("press a key to finish...");
                Console.ReadKey();
            }
        }

        private static void MessageReceived(string message)
        {
            Console.WriteLine(message);
        }

        private static void ServerDisconnectedReceived(string justification)
        {
            Console.WriteLine("You have been disconnected: " + justification);
        }
    }
}
