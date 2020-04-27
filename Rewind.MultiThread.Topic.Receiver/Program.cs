using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
namespace Rewind.MultiThread.Topic.Receiver
{
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution-21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3hGIA7ykTK3Ryj2/dGnu9XcQvaGLwbeyvZe0gMcjH44=";
        private static string _topicName = "topicmultiworker";
        private static string _subscriberName1 = "subscriberone";
        private static string _subscriberName2 = "subscribertwo";
        private static TopicClient _topicClient;
        private static SubscriptionClient _subscriberOne;
        private static SubscriptionClient _subscriberTwo;
        private static ManagementClient _managementClient;
        private static void Init()
        {
            try
            {
                _managementClient = new ManagementClient(_connectionString);
                if (!_managementClient.TopicExistsAsync(_topicName).Result)
                {
                    _managementClient.CreateTopicAsync(new TopicDescription(_topicName)
                    {
                        AutoDeleteOnIdle = TimeSpan.FromMinutes(60),
                        EnableBatchedOperations = true
                    });

                }
                _topicClient = new TopicClient(_connectionString, _topicName);
                if (!_managementClient.SubscriptionExistsAsync(_topicName, _subscriberName1).Result)
                {
                    _managementClient.CreateSubscriptionAsync(new SubscriptionDescription(_topicName, _subscriberName1)
                    {
                        AutoDeleteOnIdle = TimeSpan.FromMinutes(60),
                        EnableBatchedOperations = true
                    });

                }
                if (!_managementClient.SubscriptionExistsAsync(_topicName, _subscriberName2).Result)
                {
                    _managementClient.CreateSubscriptionAsync(new SubscriptionDescription(_topicName, _subscriberName2)
                    {
                        AutoDeleteOnIdle = TimeSpan.FromMinutes(60),
                        EnableBatchedOperations = true
                    });
                }
                _subscriberOne = new SubscriptionClient(_connectionString, _topicName, _subscriberName1);
                _subscriberTwo = new SubscriptionClient(_connectionString, _topicName, _subscriberName2);


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
        private static void Listen()
        {
            _subscriberOne.RegisterMessageHandler(SubscriberOneHandler, new MessageHandlerOptions(ErrorHandler)
            {
                AutoComplete = false,
                MaxConcurrentCalls = 100,
                MaxAutoRenewDuration = TimeSpan.FromMinutes(10)
            });
            _subscriberTwo.RegisterMessageHandler(SubscriberTwoHandler, new MessageHandlerOptions(ErrorHandler)
            {
                AutoComplete = false,
                MaxConcurrentCalls = 100,
                MaxAutoRenewDuration = TimeSpan.FromMinutes(10)
            });
            Console.WriteLine("Press ENTER to continue...");
            Console.ReadLine();
        }

        private static Task SubscriberTwoHandler(Message message, CancellationToken cancellationToken)
        {
            try
            {
                Thread.Sleep(3000);
                string text = Encoding.UTF8.GetString(message.Body);
                Console.WriteLine($"[Sub#2]-{message.MessageId}: {text}");
                _subscriberTwo.CompleteAsync(message.SystemProperties.LockToken);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return Task.CompletedTask;

        }

        private static Task SubscriberOneHandler(Message message, CancellationToken cancellationToken)
        {
            try
            {
                Thread.Sleep(3000);
                string text = Encoding.UTF8.GetString(message.Body);
                Console.WriteLine($"[Sub#1]-{message.MessageId}: {text}");
                _subscriberOne.CompleteAsync(message.SystemProperties.LockToken);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return Task.CompletedTask;
        }

        private static Task ErrorHandler(ExceptionReceivedEventArgs arg)
        {
            Console.WriteLine("Error!");
            Console.WriteLine($"Message: {arg.Exception?.Message}");
            Console.WriteLine($"EntityPath: {arg.ExceptionReceivedContext.EntityPath}");
            Console.WriteLine($"Action: {arg.ExceptionReceivedContext.Action}");
            return Task.CompletedTask;
        }

        static void Main(string[] args)
        {
            Listen();
        }
    }
}
