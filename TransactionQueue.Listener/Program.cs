using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Newtonsoft.Json;
namespace TransactionQueue.Listener
{
    class Program
    {
        private static string _queueName = "trx_queue";
        private static string _connectionString = "Endpoint=sb://mirzaevolution21.servicebus.windows.net/;SharedAccessKeyName=trx_queue_root;SharedAccessKey=TEefcXSgB0xfaLJTd28vo8w+PZhqxVTR04incCSD0G8=";
        private static QueueClient _queueClient;
        static Program()
        {
            _queueClient = new QueueClient(_connectionString, _queueName);
        }
        static async Task Init()
        {
            Console.WriteLine("** Connected to trx_queue Handler **");
            _queueClient.RegisterMessageHandler(MessageHandler, new MessageHandlerOptions(ErrorHandler)
            {
                AutoComplete = false,
                MaxConcurrentCalls = 5,
                MaxAutoRenewDuration = TimeSpan.FromMinutes(5)
            });
            Console.ReadLine();
            await _queueClient.CloseAsync();
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
            Console.WriteLine($"Receiving queue with message id: {message.MessageId}");
            Console.WriteLine(transactionDetail.ToString());

            await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        static void Main(string[] args)
        {
            Init().Wait();
        }
    }
}
