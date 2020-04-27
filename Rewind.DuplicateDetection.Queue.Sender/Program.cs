using System;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System.Threading;
using System.Collections.Generic;

namespace Rewind.DuplicateDetection.Queue.Sender
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
        static async Task Send()
        {
            try
            {
                Console.WriteLine($"Time sent: {DateTime.Now}");
                List<Message> messages = new List<Message>
                {
                    new Message(Encoding.UTF8.GetBytes("Message 1"))
                    {
                        MessageId = "1"
                    },
                    new Message(Encoding.UTF8.GetBytes("Message 2"))
                    {
                        MessageId = Guid.NewGuid().ToString()
                    },
                    new Message(Encoding.UTF8.GetBytes("Message 3"))
                    {
                        MessageId = Guid.NewGuid().ToString()
                    }
                };
                await _queueClient.SendAsync(messages);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        static void Main(string[] args)
        {
            Send().Wait();
        }
    }
}
