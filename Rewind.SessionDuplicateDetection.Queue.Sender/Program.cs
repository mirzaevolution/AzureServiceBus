using System;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System.Threading;
using System.Collections.Generic;

namespace Rewind.SessionDuplicateDetection.Queue.Sender
{
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution-21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3hGIA7ykTK3Ryj2/dGnu9XcQvaGLwbeyvZe0gMcjH44=";
        private static string _queueName = "sessionduplicatedetection";

        private static QueueClient _queueClient;

        private static void Init()
        {
            try
            {
                ManagementClient managementClient = new ManagementClient(_connectionString);
                if (!managementClient.QueueExistsAsync(_queueName).Result)
                {
                    managementClient.CreateQueueAsync(new QueueDescription(_queueName)
                    {
                        AutoDeleteOnIdle = TimeSpan.FromHours(1),
                        EnablePartitioning = true,
                        RequiresDuplicateDetection = true,
                        DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(1),
                        RequiresSession = true
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

        static async Task Send()
        {
            try
            {
                string sessionKey1 = $"Key-{Guid.NewGuid().ToString()}";
                string sessionKey2 = $"Key-{Guid.NewGuid().ToString()}";

                List<Message> list1 = new List<Message>
                {
                    new Message(Encoding.UTF8.GetBytes($"Message #1 - {sessionKey1}"))
                    {
                        MessageId = Guid.Empty.ToString(),
                        SessionId = sessionKey1,
                        PartitionKey = sessionKey1
                    },
                    new Message(Encoding.UTF8.GetBytes($"Message #2 - {sessionKey1}"))
                    {
                        MessageId = Guid.NewGuid().ToString(),
                        SessionId = sessionKey1,
                        PartitionKey = sessionKey1
                    },
                    new Message(Encoding.UTF8.GetBytes($"Message #3 - {sessionKey1}"))
                    {
                        MessageId = Guid.NewGuid().ToString(),
                        SessionId = sessionKey1,
                        PartitionKey = sessionKey1
                    },
                    new Message(Encoding.UTF8.GetBytes($"Message #4 - {sessionKey1}"))
                    {
                        MessageId = Guid.Empty.ToString(),
                        SessionId = sessionKey1,
                        PartitionKey = sessionKey1
                    }

                };
                List<Message> list2 = new List<Message>
                {
                    new Message(Encoding.UTF8.GetBytes($"Message #1 - {sessionKey2}"))
                    {
                        MessageId = Guid.Empty.ToString(),
                        SessionId = sessionKey2,
                        PartitionKey = sessionKey2
                    },
                    new Message(Encoding.UTF8.GetBytes($"Message #2 - {sessionKey2}"))
                    {
                        MessageId = Guid.NewGuid().ToString(),
                        SessionId = sessionKey2,
                        PartitionKey = sessionKey2
                    },
                    new Message(Encoding.UTF8.GetBytes($"Message #3 - {sessionKey2}"))
                    {
                        MessageId = Guid.NewGuid().ToString(),
                        SessionId = sessionKey2,
                        PartitionKey = sessionKey2
                    },
                    new Message(Encoding.UTF8.GetBytes($"Message #4 - {sessionKey2}"))
                    {
                        MessageId = Guid.Empty.ToString(),
                        SessionId = sessionKey2,
                        PartitionKey = sessionKey2
                    }

                };
                Console.WriteLine($"Sending messages with session key: {sessionKey1}");
                await _queueClient.SendAsync(list1);
                Console.WriteLine($"Sending messages with session key: {sessionKey2}");
                await _queueClient.SendAsync(list2);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        static void Main(string[] args)
        {
            Send().Wait();
            Console.WriteLine("Press ENTER to quit");
            Console.ReadLine();
        }
    }
}
