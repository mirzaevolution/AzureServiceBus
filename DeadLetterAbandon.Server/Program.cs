using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
namespace DeadLetterAbandon.Server
{
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=LrtPyL+CWb/1FFtAVgZBpgySsdaEnSQCGjaCaScnphI=";
        private static string _queueName = "corequeue";
        private static QueueClient _queueClient;
        static Program()
        {
            _queueClient = new QueueClient(_connectionString, _queueName);
        }
        private static void InitQueue()
        {
            try
            {
                _queueClient.RegisterMessageHandler(MessageHandler, new MessageHandlerOptions(ErrorHandler)
                {
                    AutoComplete = false,
                    MaxConcurrentCalls = 1
                });

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static Task ErrorHandler(ExceptionReceivedEventArgs error)
        {
            Console.WriteLine("An error occured");
            Console.WriteLine(error.Exception.ToString());
            return Task.CompletedTask;
        }

        private static async Task MessageHandler(Message message, CancellationToken cancellationToken)
        {
            string messageText = Encoding.UTF8.GetString(message.Body);
            Console.Write($"[MaxDeliveryCount:{message.SystemProperties.DeliveryCount}]");
            if (messageText.Contains("abandon", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine(" - Message abandoned");
                await _queueClient.AbandonAsync(message.SystemProperties.LockToken);
            }
            else if (messageText.Contains("deadletter", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine(" - Message cannot be processed");
                await _queueClient.DeadLetterAsync(message.SystemProperties.LockToken, "Poison Message", $"System cannot process the specified message: `{messageText}`");
            }
            else
            {
                Console.WriteLine($" - {messageText}");
                await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
            }
        }

        static void Main(string[] args)
        {
            InitQueue();
            Console.WriteLine("Press ENTER to quit");
            Console.ReadLine();
        }
    }
}
