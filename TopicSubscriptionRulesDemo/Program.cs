using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Newtonsoft.Json;

namespace TopicSubscriptionRulesDemo
{
    public class Person
    {
        public Person() { }
        public Person(
                string name,
                string job,
                string email,
                decimal salary
            )
        {
            Name = name;
            Job = job;
            Email = email;
            Salary = salary;
        }
        public string Name { get; set; }
        public string Job { get; set; }
        public string Email { get; set; }
        public decimal Salary { get; set; }
        public override string ToString()
        {
            return $"Name: {Name}, Job: {Job}, Email: {Email}, Salary: ${Salary}";
        }
    }
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=LrtPyL+CWb/1FFtAVgZBpgySsdaEnSQCGjaCaScnphI=";
        private static string _topic = "people_topic";
        private static ManagementClient _managementClient;
        private static TopicClient _topicClient;
        static Program()
        {
            _managementClient = new ManagementClient(_connectionString);
            if (!_managementClient.TopicExistsAsync(_topic).Result)
            {
                _managementClient.CreateTopicAsync(_topic).Wait();
            }
            _topicClient = new TopicClient(_connectionString, _topic);
        }
        static async Task CloseAll()
        {
            await _topicClient.CloseAsync();
            await _managementClient.CloseAsync();
        }
        static List<Person> GetPeople()
        {
            return new List<Person>
            {
                new Person("Mirza Ghulam Rasyid","Technical Leader","ghulamcyber@hotmail.com",3000m),
                new Person("Rara Anjani","Software Tester","raraanjani@gmail.com",1200m),
                new Person("Beggi Mammad","Software Developer","beggimammad@gmail.com",1850m),
                new Person("Randall Fu","Software Developer","randall95fu@hotmail.com",2100m),
                new Person("Stefan Dellano","Software Developer","stef.delano31@gmail.com",2000m),
                new Person("Michael Hawk","Cyber Security Operation Leader","black_hawk21@hotmail.com",2910m)
            };
        }
        static async Task CreateSubscriptions()
        {
            Console.WriteLine("Creating subscriptions..");
            //filter by job that ends with leader
            if (!await _managementClient.SubscriptionExistsAsync(_topic, "SubLeader"))
            {
                await _managementClient.CreateSubscriptionAsync(new SubscriptionDescription(_topic, "SubLeader")
                {
                    AutoDeleteOnIdle = TimeSpan.FromMinutes(30)
                }, new RuleDescription("FilterByLeader", new SqlFilter("job LIKE '%Leader'")));
            }

            //filter by salary that is greater than 2500
            if (!await _managementClient.SubscriptionExistsAsync(_topic, "SubHighSalary"))
            {

                await _managementClient.CreateSubscriptionAsync(new SubscriptionDescription(_topic, "SubHighSalary")
                {
                    AutoDeleteOnIdle = TimeSpan.FromMinutes(30)
                }, new RuleDescription("FilterBySalary", new SqlFilter("salary > 2500")));
            }

            //filter by job that has title = Software Developer
            if (!await _managementClient.SubscriptionExistsAsync(_topic, "SubDevJob"))
            {
                await _managementClient.CreateSubscriptionAsync(new SubscriptionDescription(_topic, "SubDevJob")
                {
                    AutoDeleteOnIdle = TimeSpan.FromMinutes(30)
                }, new RuleDescription("FilterByDevJob", new CorrelationFilter("Software Developer")));
            }

            //filter by job that has title = Software Tester
            if (!await _managementClient.SubscriptionExistsAsync(_topic, "SubTesterJob"))
            {
                await _managementClient.CreateSubscriptionAsync(new SubscriptionDescription(_topic, "SubTesterJob")
                {
                    AutoDeleteOnIdle = TimeSpan.FromMinutes(30)
                }, new RuleDescription("FilterByTesterJob", new CorrelationFilter { Label = "Software Tester" }));

            }


            //Wire-Tap
            await _managementClient.CreateSubscriptionAsync(new SubscriptionDescription(_topic, $"wire-{Guid.NewGuid().ToString()}")
            {
                AutoDeleteOnIdle = TimeSpan.FromMinutes(30),

            });

            Console.WriteLine("Done.\n");
        }
        static async Task SendMessages()
        {
            List<Person> people = GetPeople();
            List<Message> messages = new List<Message>();
            foreach (var person in people)
            {
                Message message = new Message();
                string json = JsonConvert.SerializeObject(person);
                byte[] bytesData = Encoding.UTF8.GetBytes(json);
                message.Body = bytesData;

                message.UserProperties.Add("job", person.Job);
                message.UserProperties.Add("salary", person.Salary);

                message.Label = person.Job;
                message.CorrelationId = person.Job;
                messages.Add(message);
            }
            Console.WriteLine("Sending messages..");
            await _topicClient.SendAsync(messages);
            Console.WriteLine("Done.\n");
        }
        static async Task RegisterHandlers()
        {
            IEnumerable<SubscriptionDescription> subscriptionDescriptions = await _managementClient.GetSubscriptionsAsync(_topic);

            foreach (var sub in subscriptionDescriptions)
            {
                SubscriptionClient subscriptionClient = new SubscriptionClient(_connectionString, _topic, sub.SubscriptionName);
                subscriptionClient.RegisterMessageHandler(async (message, token) =>
                {
                    Person person = JsonConvert.DeserializeObject<Person>(Encoding.UTF8.GetString(message.Body));
                    Console.WriteLine($"[{sub.SubscriptionName}] {person.ToString()}");
                    await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
                }, new MessageHandlerOptions(GlobalErrorHandler)
                {
                    AutoComplete = false,
                    MaxConcurrentCalls = 1
                });
            }

        }
        private static Task GlobalErrorHandler(ExceptionReceivedEventArgs error)
        {
            Console.WriteLine("\nError:");
            Console.WriteLine($"\t{nameof(error.ExceptionReceivedContext.Action)}: {error.ExceptionReceivedContext.Action}");
            Console.WriteLine($"\t{nameof(error.ExceptionReceivedContext.Endpoint)}: {error.ExceptionReceivedContext.Endpoint}");
            Console.WriteLine($"\t{nameof(error.ExceptionReceivedContext.EntityPath)}: {error.ExceptionReceivedContext.EntityPath}");
            Console.WriteLine("Error Messages:");
            Console.WriteLine(error.Exception.ToString());
            return Task.CompletedTask;
        }
        static async Task Init()
        {
            await CreateSubscriptions();
            Console.WriteLine("Press enter to send the messages..");
            Console.ReadLine();
            await SendMessages();
            Console.WriteLine("Press enter to register message handlers...");
            Console.ReadLine();
            await RegisterHandlers();
            Console.ReadLine();
        }
        static void Main(string[] args)
        {
            Init().Wait();
            CloseAll().Wait();
        }
    }
}
