using NetworKit.Tcp;
using System;

namespace NetworKit.ChatExample.Client
{
    class Client
    {
        static INetworkClient NetworkClient;

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

                using (NetworkClient = new TcpNetworkClient())
                {
                    NetworkClient.ConnectAsync("127.0.0.1", port, MessageReceived, "Hello !").GetAwaiter().GetResult();

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
                            NetworkClient.SendAsync(input).GetAwaiter().GetResult();
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

        private static void MessageReceived(IRemoteConnection sender, string message)
        {
            Console.WriteLine(message);
        }
    }
}
