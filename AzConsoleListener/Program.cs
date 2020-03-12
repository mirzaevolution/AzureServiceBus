using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
namespace AzConsoleListener
{
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution21.servicebus.windows.net/;SharedAccessKeyName=listener;SharedAccessKey=RIXo30zK7s7biVK9uqZrPzqmwV/9FHamnjOuO2WXV0c=;";
        private static string _topicName = "az_one_topic";
        private static string _subscriptionName = "az_console_sub";
        private static SubscriptionClient _subscriptionClient;
        static Program()
        {
            _subscriptionClient = new SubscriptionClient(_connectionString, _topicName, _subscriptionName);
        }
        static async Task Init()
        {
            _subscriptionClient.RegisterMessageHandler(MessageHandler,
                    new MessageHandlerOptions(ErrorHandler)
                    {
                        AutoComplete = false,
                        MaxConcurrentCalls = 5
                    }
                );
            Console.ReadLine();
            await _subscriptionClient.CloseAsync();
        }

        private static Task ErrorHandler(ExceptionReceivedEventArgs error)
        {
            Console.WriteLine($"Error: {error.Exception?.Message}");
            return Task.CompletedTask;
        }

        private static async Task MessageHandler(Message message, CancellationToken cancellationToken)
        {
            string jsonData = Encoding.UTF8.GetString(message.Body);
            TransactionDetail transactionDetail = JsonConvert.DeserializeObject<TransactionDetail>(jsonData);
            Console.WriteLine($"\nProcessing information with message id: {message.MessageId}");
            Console.WriteLine(transactionDetail.ToString());
            await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        static void Main(string[] args)
        {
            Init().Wait();
        }
    }
}
