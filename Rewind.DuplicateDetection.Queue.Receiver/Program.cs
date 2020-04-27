using System;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System.Threading;
using System.Collections.Generic;

namespace Rewind.DuplicateDetection.Queue.Receiver
{
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution-21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3hGIA7ykTK3Ryj2/dGnu9XcQvaGLwbeyvZe0gMcjH44=";
        private static string _queueName = "rewindduplicatedetection";
        private static QueueClient _queueClient;
        private static ManagementClient _managementClient;
        static Program()
        {
            Init();
        }
        private static void Init()
        {
            try
            {
                _managementClient = new ManagementClient(_connectionString);
                if (!_managementClient.QueueExistsAsync(_queueName).Result)
                {
                    _managementClient.CreateQueueAsync(new QueueDescription(_queueName)
                    {
                        AutoDeleteOnIdle = TimeSpan.FromMinutes(60),
                        EnableBatchedOperations = true,
                        RequiresDuplicateDetection = true,
                        DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(2)
                    });
                }
                _queueClient = new QueueClient(_connectionString, _queueName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        static void Listen()
        {
            _queueClient.RegisterMessageHandler(MessageHandler, new MessageHandlerOptions(ErrorHandler)
            {
                AutoComplete = false,
                MaxConcurrentCalls = 100,
                MaxAutoRenewDuration = TimeSpan.FromMinutes(10)
            });
            Console.WriteLine("Press ENTER to quit");
            Console.ReadLine();
        }

        private static Task ErrorHandler(ExceptionReceivedEventArgs arg)
        {
            Console.WriteLine($"Error: {arg.Exception.Message}");
            return Task.CompletedTask;
        }

        private static async Task MessageHandler(Message message, CancellationToken cancellationToken)
        {
            string messageId = message.MessageId;
            string text = Encoding.UTF8.GetString(message.Body);
            Console.WriteLine($"[{DateTime.Now}] {messageId}> {text}");
            await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        static void Main(string[] args)
        {
            Listen();
        }
    }
}
