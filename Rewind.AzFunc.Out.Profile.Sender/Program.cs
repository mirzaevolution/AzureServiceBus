using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Newtonsoft.Json;
namespace Rewind.AzFunc.Out.Profile.Sender
{
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution-21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3hGIA7ykTK3Ryj2/dGnu9XcQvaGLwbeyvZe0gMcjH44=";
        private static string _topicName = "azfuncprofiletopic";
        private static TopicClient _senderTopicClient;

        static Program()
        {
            _senderTopicClient = new TopicClient(_connectionString, _topicName);
        }
        static async Task Send(string name = "name", string job = "job")
        {
            Profile profile = new Profile
            {
                Name = name,
                Job = job
            };
            Message message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(profile)))
            {
                MessageId = Guid.NewGuid().ToString()
            };
            await _senderTopicClient.SendAsync(message);
        }
        static void Main(string[] args)
        {
            Console.Write("Enter your name: ");
            string name = Console.ReadLine();
            Console.Write("Enter your job: ");
            string job = Console.ReadLine();
            Send(name, job).Wait();
            Console.ReadLine();
        }
    }
}