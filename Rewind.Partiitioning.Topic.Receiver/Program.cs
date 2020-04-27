using System;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System.Threading;
using System.Collections.Generic;
namespace Rewind.Partiitioning.Topic.Receiver
{
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution-21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3hGIA7ykTK3Ryj2/dGnu9XcQvaGLwbeyvZe0gMcjH44=";
        private static string _topicName = "partitionedtopic";
        private static TopicClient _topicClient;
        private static ManagementClient _managementClient;
        private static SubscriptionClient _subscriptionClient;
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
                string subscriptionName = Guid.NewGuid().ToString();
                if (!_managementClient.SubscriptionExistsAsync(_topicName, subscriptionName).Result)
                {
                    _managementClient.CreateSubscriptionAsync(new SubscriptionDescription(_topicName, subscriptionName)
                    {
                        AutoDeleteOnIdle = TimeSpan.FromMinutes(45),
                        EnableBatchedOperations = true
                    }).Wait();
                }
                _subscriptionClient = new SubscriptionClient(_connectionString, _topicName, subscriptionName);
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
            try
            {
                _subscriptionClient.RegisterMessageHandler(MessageHandler, new MessageHandlerOptions(ErrorHandler)
                {
                    AutoComplete = false,
                    MaxConcurrentCalls = 20,
                    MaxAutoRenewDuration = TimeSpan.FromMinutes(10)
                });
                Console.WriteLine("Press ENTER to quit");
                Console.ReadLine();
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
            try
            {
                string text = Encoding.UTF8.GetString(message.Body);
                Console.WriteLine($"[@{DateTime.Now}, %{message.PartitionKey}, *{message.MessageId}] {text}");
                await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static void Main(string[] args)
        {
            Listen();

        }
    }
}
