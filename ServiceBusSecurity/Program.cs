using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
namespace ServiceBusSecurity
{
    class Program
    {
        private static string _coreConnectionString = "Endpoint=sb://mirzaevolution21.servicebus.windows.net/;SharedAccessKeyName=core_policy;SharedAccessKey=iVQkUwTLX3OnAjgnb9h2PfntujJ6VCm1QNT+Qg8WwH8=;";

        private static string _sendConnectionString = "Endpoint=sb://mirzaevolution21.servicebus.windows.net/;SharedAccessKeyName=msc_send_policy;SharedAccessKey=uxIszQRN8QGou03QGsYsxDSK1lt3ndky01fqOKwkqCQ=;";
        private static string _topicName = "msc_topic";

        private static string _seSubName = "se_sub";
        private static string _seConnectionString = "Endpoint=sb://mirzaevolution21.servicebus.windows.net/;SharedAccessKeyName=se_sub_policy;SharedAccessKey=50Ff35UOUkMGtY9sCkyl1/gifgDK6tR6ufRZsenF1Bk=;";

        private static string _finSubName = "fin_sub";
        private static string _finConnectionString = "Endpoint=sb://mirzaevolution21.servicebus.windows.net/;SharedAccessKeyName=fin_sub_policy;SharedAccessKey=mSUEfn6GykOkMmKLWY78rwm022siIuS2b0LoPds/iaA=;";

        private static string _hrSubName = "hr_sub";
        private static string _hrConnectionString = "Endpoint=sb://mirzaevolution21.servicebus.windows.net/;SharedAccessKeyName=hr_sub_policy;SharedAccessKey=YDRlfCogMrK0tYX1R/PN52K5rb/cBy6JAOy0w59lcz8=;";

        private static ManagementClient _managementClient;

        private static TopicClient _sendTopicClient;
        static Program()
        {
            _managementClient = new ManagementClient(_coreConnectionString);
            _sendTopicClient = new TopicClient(_sendConnectionString, _topicName);
            if (!_managementClient.SubscriptionExistsAsync(_topicName, _seSubName).Result)
            {
                RuleDescription seSubRule = new RuleDescription(nameof(seSubRule), new SqlFilter("department = 'Software Engineering'"));
                SubscriptionDescription seSubDesc = new SubscriptionDescription(_topicName, _seSubName);
                _managementClient.CreateSubscriptionAsync(seSubDesc, seSubRule).Wait();
            }
            if (!_managementClient.SubscriptionExistsAsync(_topicName, _finSubName).Result)
            {
                RuleDescription finSubRule = new RuleDescription(nameof(finSubRule), new SqlFilter("department = 'Finance'"));

                SubscriptionDescription finSubDesc = new SubscriptionDescription(_topicName, _finSubName);
                _managementClient.CreateSubscriptionAsync(finSubDesc, finSubRule).Wait();
            }
            if (!_managementClient.SubscriptionExistsAsync(_topicName, _hrSubName).Result)
            {
                RuleDescription hrSubRule = new RuleDescription(nameof(hrSubRule), new SqlFilter("department = 'Human Resources'"));
                SubscriptionDescription hrSubDesc = new SubscriptionDescription(_topicName, _hrSubName);
                _managementClient.CreateSubscriptionAsync(hrSubDesc, hrSubRule).Wait();
            }
        }
        private static async Task Init()
        {
            try
            {
                Console.WriteLine("Registering message handlers...");

                IEnumerable<SubscriptionDescription> subscriptionDescriptions = await _managementClient.GetSubscriptionsAsync(_topicName);

                foreach (var sub in subscriptionDescriptions)
                {
                    string connectionString = string.Empty;
                    if (sub.SubscriptionName.Equals(_seSubName))
                    {
                        connectionString = _seConnectionString;
                    }
                    else if (sub.SubscriptionName.Equals(_hrSubName))
                    {
                        connectionString = _hrConnectionString;
                    }
                    else if (sub.SubscriptionName.Equals(_finSubName))
                    {
                        connectionString = _finConnectionString;
                    }
                    SubscriptionClient subscriptionClient = new SubscriptionClient(connectionString, _topicName, sub.SubscriptionName);

                    subscriptionClient.RegisterMessageHandler((message, token) =>
                    {
                        string messageText = Encoding.UTF8.GetString(message.Body);
                        Console.WriteLine($"[{message.UserProperties["department"]}] Message: {messageText}");
                        return Task.CompletedTask;
                    }, new MessageHandlerOptions((error) =>
                    {

                        Console.WriteLine($"Error: {error.Exception?.Message}");
                        return Task.CompletedTask;
                    })
                    {
                        MaxConcurrentCalls = 1
                    });
                }


                var list = new[]
                {
                    new { Message = "Mirza is now a new Software Architect", Department = "Software Engineering" },
                    new { Message = "Randall Fu has been assigned in new Project", Department = "Software Engineering" },
                    new { Message = "New salary increase for July payment", Department = "Finance" },
                    new { Message = "Our CTO will receive reward from Microsoft", Department = "Software Engineering" },
                    new { Message = "New .NET team will be setup next month", Department = "Human Resources" },
                    new { Message = "MSC will hire massive amount of students from UI", Department = "Human Resources" }
                };
                List<Message> messages = new List<Message>();
                foreach (var message in list)
                {
                    Message messagePayload = new Message(Encoding.UTF8.GetBytes(message.Message));
                    messagePayload.UserProperties.Add("department", message.Department);
                    messages.Add(messagePayload);
                }
                Console.WriteLine("Sending messages\n");
                await _sendTopicClient.SendAsync(messages);



            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }



        static void Main(string[] args)
        {
            Init().Wait();
            Console.ReadLine();
        }
    }
}
