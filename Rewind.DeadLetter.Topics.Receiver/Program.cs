using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

namespace Rewind.DeadLetter.Topics.Receiver
{
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution-21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3hGIA7ykTK3Ryj2/dGnu9XcQvaGLwbeyvZe0gMcjH44=";
        private static string _topicName = "corequeue2";
        private static string _subscriptionName = "sub001";
        private static TopicClient _senderTopicClient;
        private static SubscriptionClient _subscriptionClient;
        private static SubscriptionClient _deadLetterSubscriptionClient;
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
                if (!_managementClient.SubscriptionExistsAsync(_topicName, _subscriptionName).Result)
                {
                    _managementClient.CreateSubscriptionAsync(new SubscriptionDescription(_topicName, _subscriptionName)
                    {
                        AutoDeleteOnIdle = TimeSpan.FromMinutes(45),
                        EnableDeadLetteringOnMessageExpiration = true
                    }).Wait();
                }
                string deadLetterPath = EntityNameHelper.FormatDeadLetterPath(_subscriptionName);
                _subscriptionClient = new SubscriptionClient(_connectionString, _topicName, _subscriptionName);
                _deadLetterSubscriptionClient = new SubscriptionClient(_connectionString, _topicName, deadLetterPath);

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
        private static void ReceiveMessages()
        {
            _subscriptionClient.RegisterMessageHandler(OnMessageReceived, new MessageHandlerOptions(OnErrorHandler)
            {
                AutoComplete = false
            });
            _deadLetterSubscriptionClient.RegisterMessageHandler(OnDeadLetterMessageReceived, OnErrorHandler);
        }

        private static Task OnDeadLetterMessageReceived(Message message, CancellationToken token)
        {
            Console.WriteLine("**  Dead Letter Message Received **");
            string errorText = Encoding.UTF8.GetString(message.Body);
            Console.WriteLine($"Error: {errorText}");
            foreach (var item in message.UserProperties)
            {
                Console.WriteLine($"{item.Key}: {item.Value}");
            }
            return Task.CompletedTask;
        }

        private static Task OnErrorHandler(ExceptionReceivedEventArgs arg)
        {
            Console.WriteLine($"Error: {arg.Exception.Message}");
            return Task.CompletedTask;
        }

        private static async Task OnMessageReceived(Message message, CancellationToken token)
        {
            string messageText = Encoding.UTF8.GetString(message.Body);
            if (messageText.Contains("deadletter"))
            {
                await _subscriptionClient.DeadLetterAsync(message.SystemProperties.LockToken, "Invalid message", "We cannot process the incoming message");
            }
            else
            {
                Console.WriteLine($"[*] {messageText}");
                await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
            }
        }

        static void Main(string[] args)
        {
            ReceiveMessages();
            Console.WriteLine("Press ENTER to quit");
            Console.ReadLine();
        }
    }
}
