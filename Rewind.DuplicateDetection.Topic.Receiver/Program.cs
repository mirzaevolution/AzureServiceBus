using System;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System.Threading;
using System.Collections.Generic;

namespace Rewind.DuplicateDetection.Topic.Receiver
{
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution-21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3hGIA7ykTK3Ryj2/dGnu9XcQvaGLwbeyvZe0gMcjH44=";
        private static string _topicName = "core";
        private static TopicClient _topicClient;
        private static ManagementClient _managementClient;
        private static SubscriptionClient _subscriptionClient;

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
                string subscriberName = Guid.NewGuid().ToString();
                await _managementClient.CreateSubscriptionAsync(new SubscriptionDescription(_topicName, subscriberName)
                {
                    AutoDeleteOnIdle = TimeSpan.FromMinutes(45)
                });
                _subscriptionClient = new SubscriptionClient(_connectionString, _topicName, subscriberName);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static Task ErrorHandler(ExceptionReceivedEventArgs arg)
        {
            Console.WriteLine($"Error: {arg.Exception.Message}");
            return Task.CompletedTask;
        }

        private static async Task MessageHandler(Message message, CancellationToken cancellationToken)
        {
            string messageText = Encoding.UTF8.GetString(message.Body);
            Console.WriteLine($"[{DateTime.Now}] ({message.MessageId}) {messageText}");
            await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
        }


        static void Main(string[] args)
        {
            Init().Wait();
            _subscriptionClient.RegisterMessageHandler(MessageHandler, new MessageHandlerOptions(ErrorHandler)
            {
                AutoComplete = false,
                MaxConcurrentCalls = 20,
                MaxAutoRenewDuration = TimeSpan.FromMinutes(10)
            });
            Console.WriteLine("Press ENTER to quit");
            Console.ReadLine();
        }
    }
}
