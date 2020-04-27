using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
namespace Intro.ChatTopic
{
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=LrtPyL+CWb/1FFtAVgZBpgySsdaEnSQCGjaCaScnphI=";
        private static string _topicName = "ChatTopic";
        private static string _userName = "";
        private static ManagementClient _managementClient;
        static Program()
        {
            _managementClient = new ManagementClient(_connectionString);

        }
        static async Task Init(string userName)
        {
            try
            {
                _userName = userName;
                if (!await _managementClient.TopicExistsAsync(_topicName))
                {
                    await _managementClient.CreateTopicAsync(new TopicDescription(_topicName) { AutoDeleteOnIdle = TimeSpan.FromMinutes(6) });

                }
                if (!await _managementClient.SubscriptionExistsAsync(_topicName, userName))
                {
                    SubscriptionDescription subscriptionDescription = new SubscriptionDescription(_topicName, userName)
                    {
                        AutoDeleteOnIdle = TimeSpan.FromMinutes(5)
                    };
                    await _managementClient.CreateSubscriptionAsync(subscriptionDescription);


                }
                TopicClient topicClient = new TopicClient(_connectionString, _topicName);
                SubscriptionClient subscriptionClient = new SubscriptionClient(_connectionString, _topicName, userName);
                subscriptionClient.RegisterMessageHandler(MessageHandler, ExceptionHandler);
                Message message = new Message(Encoding.UTF8.GetBytes($"Entering chat..."));
                message.Label = userName;
                await topicClient.SendAsync(message);
                while (true)
                {
                    string input = Console.ReadLine().Trim();
                    if (input.Equals("exit", StringComparison.InvariantCultureIgnoreCase))
                    {
                        break;
                    }
                    message = new Message(Encoding.UTF8.GetBytes(input));
                    message.Label = userName;
                    await topicClient.SendAsync(message);
                }
                message = new Message(Encoding.UTF8.GetBytes("Leaving chat..."));
                message.Label = userName;
                await topicClient.SendAsync(message);
                await subscriptionClient.CloseAsync();
                await topicClient.CloseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static Task ExceptionHandler(ExceptionReceivedEventArgs exception)
        {
            Console.WriteLine($"[!] {exception.Exception?.Message}");
            return Task.CompletedTask;

        }

        private static Task MessageHandler(Message message, CancellationToken cancellationToken)
        {
            if (!message.Label.Equals(_userName))
            {

                string messageBody = Encoding.UTF8.GetString(message.Body);
                Console.WriteLine($"{message.Label}> {messageBody}");
            }
            return Task.CompletedTask;
        }

        static void Main(string[] args)
        {
            Console.Write("Enter username: ");
            string username = Console.ReadLine();
            Init(username).Wait();

        }
    }
}
