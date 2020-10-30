using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;


namespace Rewind.DeadLetter.Topics.Sender
{
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution-21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3hGIA7ykTK3Ryj2/dGnu9XcQvaGLwbeyvZe0gMcjH44=";
        private static string _topicName = "corequeue2";
        private static TopicClient _senderTopicClient;
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
                        AutoDeleteOnIdle = TimeSpan.FromHours(1)
                    }).Wait();
                }
                _senderTopicClient = new TopicClient(_connectionString, _topicName);

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
        static async Task Send(string message = "Hello World")
        {
            Console.Write("Sending message...");
            Message messagePayload = new Message(Encoding.UTF8.GetBytes(message));
            await _senderTopicClient.SendAsync(messagePayload);
            Console.WriteLine("OK");
        }
        static void Main(string[] args)
        {
            Console.Write("> ");
            string message = Console.ReadLine();
            Send(message).Wait();
            Console.ReadLine();
        }
    }
}
