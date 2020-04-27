using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
namespace Rewind.MultiThread.Topic.Sender
{
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution-21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3hGIA7ykTK3Ryj2/dGnu9XcQvaGLwbeyvZe0gMcjH44=";
        private static string _topicName = "topicmultiworker";
        private static TopicClient _topicClient;
        private static ManagementClient _managementClient;
        private static void Init()
        {
            try
            {
                _managementClient = new ManagementClient(_connectionString);
                if (!_managementClient.TopicExistsAsync(_topicName).Result)
                {
                    _managementClient.CreateTopicAsync(new TopicDescription(_topicName)
                    {
                        AutoDeleteOnIdle = TimeSpan.FromMinutes(60),
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
        private static async Task Send(List<string> items)
        {
            try
            {
                List<Message> messages = new List<Message>();
                foreach (var item in items)
                {
                    Message message = new Message(Encoding.UTF8.GetBytes(item));
                    message.MessageId = Guid.NewGuid().ToString();
                    messages.Add(message);
                }
                Console.WriteLine("Sending messages...");
                await _topicClient.SendAsync(messages);
                Console.WriteLine("Finished...");
                Console.WriteLine("Press ENTER to exit");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Console.ReadLine();
            }
        }
        static void Main(string[] args)
        {
            List<string> messages = new List<string>();
            foreach (var number in Enumerable.Range(1, 10))
            {
                messages.Add($"Message {number}");
            }
            Send(messages).Wait();
        }
    }
}
