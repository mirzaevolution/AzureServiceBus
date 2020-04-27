using System;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Rewind.PubSub.SqlLike.Sender
{
    public class Profile
    {
        public Profile(string name, string job, string country)
        {
            this.Name = name;
            this.Job = job;
            this.Country = country;
        }
        public string Name { get; set; }
        public string Job { get; set; }
        public string Country { get; set; }
    }
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution-21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3hGIA7ykTK3Ryj2/dGnu9XcQvaGLwbeyvZe0gMcjH44=";
        private static string _topicName = "pubsub";
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
            List<Profile> profiles = new List<Profile>
            {
                new Profile("Mirza Ghulam Rasyid","Developer","ID"),
                new Profile("Rara Anjani","Developer","ID"),
                new Profile("Randall Fu","Hacker","ID"),
                new Profile("Koyuki","Developer","JP"),
                new Profile("Michael Hawk","Developer","US"),
                new Profile("Jason Morales","Hacker","US")
            };
            List<Message> messages = new List<Message>();
            string partitionKey = Guid.NewGuid().ToString();
            foreach (Profile profile in profiles)
            {
                string json = JsonConvert.SerializeObject(profile);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                Message message = new Message(jsonBytes);
                message.PartitionKey = partitionKey;
                message.MessageId = Guid.NewGuid().ToString();
                message.UserProperties.Add("Name", profile.Name);
                message.UserProperties.Add("Job", profile.Job);
                message.UserProperties.Add("Country", profile.Country);
                messages.Add(message);
            }
            await _topicClient.SendAsync(messages);
        }
        static void Main(string[] args)
        {
            Send().Wait();
        }
    }
}
