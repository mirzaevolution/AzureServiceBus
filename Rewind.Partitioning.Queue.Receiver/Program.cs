using System;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System.Threading;
using System.Collections.Generic;
namespace Rewind.Partitioning.Queue.Receiver
{
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution-21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3hGIA7ykTK3Ryj2/dGnu9XcQvaGLwbeyvZe0gMcjH44=";
        private static string _queueName = "partitionedqueue";
        private static QueueClient _queueClient;
        private static ManagementClient _managementClient;
        private static void Init()
        {
            try
            {
                _managementClient = new ManagementClient(_connectionString);
                if (!_managementClient.QueueExistsAsync(_queueName).Result)
                {
                    _managementClient.CreateQueueAsync(new QueueDescription(_queueName)
                    {
                        EnablePartitioning = true,
                        RequiresDuplicateDetection = true,
                        DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(2)
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
        static void RegisterHandler()
        {
            _queueClient.RegisterMessageHandler(MessageHandler, new MessageHandlerOptions(ErrorHandler)
            {
                AutoComplete = false,
                MaxConcurrentCalls = 10,
                MaxAutoRenewDuration = TimeSpan.FromMinutes(10)
            });
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }

        private static async Task MessageHandler(Message message, CancellationToken token)
        {
            string text = Encoding.UTF8.GetString(message.Body);
            Console.WriteLine($"[@{DateTime.Now}, %{message.PartitionKey}, *{message.MessageId}] {text}");
            await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        private static Task ErrorHandler(ExceptionReceivedEventArgs arg)
        {
            Console.WriteLine("ERROR");
            Console.WriteLine(arg.Exception.ToString());
            return Task.CompletedTask;
        }

        static void Main(string[] args)
        {
            RegisterHandler();
        }
    }
}
