using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

namespace Rewind.DeadLetter.Queue.Receiver
{
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution-21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3hGIA7ykTK3Ryj2/dGnu9XcQvaGLwbeyvZe0gMcjH44=";
        private static string _queueName = "corequeue1";
        private static QueueClient _queueClient;
        private static ManagementClient _managementClient;
        static void Init()
        {
            try
            {
                _managementClient = new ManagementClient(_connectionString);
                if (!_managementClient.QueueExistsAsync(_queueName).Result)
                {
                    _managementClient.CreateQueueAsync(new QueueDescription(_queueName)
                    {
                        AutoDeleteOnIdle = TimeSpan.FromHours(1),
                        EnableDeadLetteringOnMessageExpiration = true
                    }).Wait();
                }
                _queueClient = new QueueClient(_connectionString, _queueName);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        static Program()
        {
            Init();
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
            Console.Write($"[DeliveryCount: {message.SystemProperties.DeliveryCount}]");
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
