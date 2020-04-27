using System;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;
namespace Rewind.PubSub.SqlLike.Receiver
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
        private static SubscriptionClient _subscriberByDeveloperJob;
        private static SubscriptionClient _subscriberByIDCountry;
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

                string subscriberByDevJob = Guid.NewGuid().ToString();
                RuleDescription ruleDescriptionByDevJob = new RuleDescription("DevJobRule", new SqlFilter("Job = 'Developer'"));
                if (!_managementClient.SubscriptionExistsAsync(_topicName, subscriberByDevJob).Result)
                {
                    _managementClient.CreateSubscriptionAsync(new SubscriptionDescription(_topicName, subscriberByDevJob)
                    {
                        AutoDeleteOnIdle = TimeSpan.FromMinutes(40),
                        EnableBatchedOperations = true
                    }, ruleDescriptionByDevJob).Wait();
                }

                string subscriberByIDCountry = Guid.NewGuid().ToString();
                RuleDescription ruleDescriptionByIDCountry = new RuleDescription("IDCountryRule", new SqlFilter("Country = 'ID'"));
                if (!_managementClient.SubscriptionExistsAsync(_topicName, subscriberByIDCountry).Result)
                {
                    _managementClient.CreateSubscriptionAsync(new SubscriptionDescription(_topicName, subscriberByIDCountry)
                    {
                        AutoDeleteOnIdle = TimeSpan.FromMinutes(40),
                        EnableBatchedOperations = true
                    }, ruleDescriptionByIDCountry).Wait();
                }


                _subscriberByDeveloperJob = new SubscriptionClient(_connectionString, _topicName, subscriberByDevJob);
                _subscriberByIDCountry = new SubscriptionClient(_connectionString, _topicName, subscriberByIDCountry);


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
        static void Listen()
        {
            _subscriberByDeveloperJob.RegisterMessageHandler(DevMessageHandler, new MessageHandlerOptions(ErrorHandler)
            {
                AutoComplete = false,
                MaxConcurrentCalls = 10,
                MaxAutoRenewDuration = TimeSpan.FromMinutes(10)
            });
            _subscriberByIDCountry.RegisterMessageHandler(IDMessageHandler, new MessageHandlerOptions(ErrorHandler)
            {
                AutoComplete = false,
                MaxConcurrentCalls = 10,
                MaxAutoRenewDuration = TimeSpan.FromMinutes(10)
            });
            Console.WriteLine("Press ENTER to quit");
            Console.ReadLine();
        }

        private static async Task IDMessageHandler(Message message, CancellationToken cancellationToken)
        {
            string text = Encoding.UTF8.GetString(message.Body);
            Profile profile = JsonConvert.DeserializeObject<Profile>(text);
            Console.WriteLine($"Name: {profile.Name}");
            Console.WriteLine($"Job: {profile.Job}");
            Console.WriteLine($"Country: {profile.Country}\n");
            await _subscriberByIDCountry.CompleteAsync(message.SystemProperties.LockToken);
        }

        private static Task ErrorHandler(ExceptionReceivedEventArgs arg)
        {
            Console.WriteLine($"Error: {arg.Exception.Message}");
            return Task.CompletedTask;
        }

        private static async Task DevMessageHandler(Message message, CancellationToken cancellationToken)
        {
            string text = Encoding.UTF8.GetString(message.Body);
            Profile profile = JsonConvert.DeserializeObject<Profile>(text);
            Console.WriteLine($"Name: {profile.Name}");
            Console.WriteLine($"Job: {profile.Job}");
            Console.WriteLine($"Country: {profile.Country}\n");
            await _subscriberByDeveloperJob.CompleteAsync(message.SystemProperties.LockToken);
        }

        static void Main(string[] args)
        {
            Listen();

        }
    }
}
