using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

namespace DeadLetterAbandon.Client
{

    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution-21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3hGIA7ykTK3Ryj2/dGnu9XcQvaGLwbeyvZe0gMcjH44=";

        private static string _queueName = "corequeue0";
        private static QueueClient _queueClient;
        static Program()
        {
            _queueClient = new QueueClient(_connectionString, _queueName);
        }
        private static async Task SendMessage(string input)
        {
            try
            {
                Message message = new Message(Encoding.UTF8.GetBytes(input));
                await _queueClient.SendAsync(message);
                Console.WriteLine("Message sent.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        static void Main(string[] args)
        {
            while (true)
            {

                Console.Write(">> ");
                string input = Console.ReadLine();
                if (input.Equals("exit", StringComparison.InvariantCultureIgnoreCase))
                {
                    break;
                }
                SendMessage(input).Wait();
            }
            Console.ReadLine();

        }
    }
}
