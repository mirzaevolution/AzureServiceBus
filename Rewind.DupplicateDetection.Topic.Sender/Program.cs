using System;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System.Threading;
using System.Collections.Generic;


namespace Rewind.DupplicateDetection.Topic.Sender
{
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution-21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3hGIA7ykTK3Ryj2/dGnu9XcQvaGLwbeyvZe0gMcjH44=";
        private static string _topicName = "core";
        private static TopicClient _topicClient;
        private static ManagementClient _managementClient;

        private static async Task Init()
        {
            try
            {
                _managementClient = new ManagementClient(_connectionString);
                if (!await _managementClient.TopicExistsAsync(_topicName))
                {
                    await _managementClient.CreateTopicAsync(new TopicDescription(_topicName)
                    {
                        AutoDeleteOnIdle = TimeSpan.FromHours(1),
                        DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(2),
                        RequiresDuplicateDetection = true
                    });
                }
                _topicClient = new TopicClient(_connectionString, _topicName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        private static async Task Sender()
        {
            List<Message> messages = new List<Message>()
            {
                new Message(Encoding.UTF8.GetBytes("Message #1"))
                {
                    MessageId = Guid.Empty.ToString()
                },
                new Message(Encoding.UTF8.GetBytes("Message #2"))
                {
                    MessageId = Guid.NewGuid().ToString()
                },
                new Message(Encoding.UTF8.GetBytes("Message #3"))
                {
                    MessageId = Guid.NewGuid().ToString()
                }
            };
            Console.WriteLine("Sending messages...");
            await _topicClient.SendAsync(messages);
            Console.WriteLine($"Messages have been sent at {DateTime.Now}.");
        }
        static void Main(string[] args)
        {
            Init().Wait();
            Sender().Wait();
            Console.WriteLine("Press ENTER to quit");
            Console.ReadLine();
        }
    }
}
