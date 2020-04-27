using System;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System.Threading;
using System.Collections.Generic;
namespace Rewind.Partitioning.Topic.Sender
{
    class Program
    {

        private static string _connectionString = "Endpoint=sb://mirzaevolution-21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3hGIA7ykTK3Ryj2/dGnu9XcQvaGLwbeyvZe0gMcjH44=";
        private static string _topicName = "partitionedtopic";
        private static TopicClient _topicClient;
        private static ManagementClient _managementClient;

        static void Init()
        {
            try
            {
                _managementClient = new ManagementClient(_connectionString);
                if (!_managementClient.TopicExistsAsync(_topicName).Result)
                {
                    _managementClient.CreateTopicAsync(new TopicDescription(_topicName)
                    {
                        AutoDeleteOnIdle = TimeSpan.FromHours(1),
                        EnablePartitioning = true,
                        RequiresDuplicateDetection = true,
                        DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(2),
                        EnableBatchedOperations = true
                    });
                }
                _topicClient = new TopicClient(_connectionString, _topicName);
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
                string partitionKey1 = "P-KEY-ONE";
                string partitionKey2 = "P-KEY-TWO";

                List<Message> messages = new List<Message>
                {
                    new Message(Encoding.UTF8.GetBytes($"Message #1"))
                    {
                        MessageId = Guid.Empty.ToString(),
                        PartitionKey = partitionKey1,
                    },
                    new Message(Encoding.UTF8.GetBytes("Message #2"))
                    {
                        MessageId = Guid.NewGuid().ToString(),
                        PartitionKey = partitionKey1
                    },
                    new Message(Encoding.UTF8.GetBytes("Message #3"))
                    {
                        MessageId = Guid.NewGuid().ToString(),
                        PartitionKey = partitionKey1
                    },
                     new Message(Encoding.UTF8.GetBytes($"Message #4"))
                    {
                        MessageId = Guid.Empty.ToString(),
                        PartitionKey = partitionKey1,
                    },
                };

                Console.WriteLine("Sending all messages...");
                await _topicClient.SendAsync(messages);
                Console.WriteLine($"Messages with partition key: {partitionKey1} have been sent @{DateTime.Now}");

                messages = new List<Message>
                {
                    new Message(Encoding.UTF8.GetBytes($"Message #1"))
                    {
                        MessageId = Guid.Empty.ToString(),
                        PartitionKey = partitionKey2
                    },
                    new Message(Encoding.UTF8.GetBytes("Message #2"))
                    {
                        MessageId = Guid.NewGuid().ToString(),
                        PartitionKey = partitionKey2
                    },
                    new Message(Encoding.UTF8.GetBytes("Message #3"))
                    {
                        MessageId = Guid.NewGuid().ToString(),
                        PartitionKey = partitionKey2
                    },
                     new Message(Encoding.UTF8.GetBytes($"Message #4"))
                    {
                        MessageId = Guid.Empty.ToString(),
                        PartitionKey = partitionKey2
                    },
                };
                Console.WriteLine("Sending all messages...");
                await _topicClient.SendAsync(messages);
                Console.WriteLine($"Messages with partition key: {partitionKey2} have been sent @{DateTime.Now}");
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
