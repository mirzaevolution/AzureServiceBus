using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;

namespace ManageArtefacts
{
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=LrtPyL+CWb/1FFtAVgZBpgySsdaEnSQCGjaCaScnphI=";
        private static ManagementClient _managementClient;

        static Program()
        {
            _managementClient = new ManagementClient(_connectionString);
        }
        static void PrintQueueDescription(QueueDescription queue)
        {
            Console.WriteLine($"{nameof(queue.Path)}: {queue.Path}");
            Console.WriteLine($"{nameof(queue.DefaultMessageTimeToLive)}: {queue.DefaultMessageTimeToLive}");
            Console.WriteLine($"{nameof(queue.DuplicateDetectionHistoryTimeWindow)}: {queue.DuplicateDetectionHistoryTimeWindow}");
            Console.WriteLine($"{nameof(queue.EnableBatchedOperations)}: {queue.EnableBatchedOperations}");
            Console.WriteLine($"{nameof(queue.EnableDeadLetteringOnMessageExpiration)}: {queue.EnableDeadLetteringOnMessageExpiration}");
            Console.WriteLine($"{nameof(queue.EnablePartitioning)}: {queue.EnablePartitioning}");
            Console.WriteLine($"{nameof(queue.LockDuration)}: {queue.LockDuration}");
            Console.WriteLine($"{nameof(queue.MaxDeliveryCount)}: {queue.MaxDeliveryCount}");
            Console.WriteLine($"{nameof(queue.MaxSizeInMB)}: {queue.MaxSizeInMB}");
            Console.WriteLine($"{nameof(queue.RequiresDuplicateDetection)}: {queue.RequiresDuplicateDetection}");
            Console.WriteLine($"{nameof(queue.RequiresSession)}: {queue.RequiresSession}");
            Console.WriteLine($"{nameof(queue.Status)}: {queue.Status}");
            Console.WriteLine($"{nameof(queue.UserMetadata)}: {queue.UserMetadata}");
            Console.WriteLine();
        }
        static void PrintTopicDescription(TopicDescription topic)
        {
            Console.WriteLine($"{nameof(topic.Path)}: {topic.Path}");
            Console.WriteLine($"{nameof(topic.AutoDeleteOnIdle)}: {topic.AutoDeleteOnIdle}");
            Console.WriteLine($"{nameof(topic.DefaultMessageTimeToLive)}: {topic.DefaultMessageTimeToLive}");
            Console.WriteLine($"{nameof(topic.DuplicateDetectionHistoryTimeWindow)}: {topic.DuplicateDetectionHistoryTimeWindow}");
            Console.WriteLine($"{nameof(topic.EnableBatchedOperations)}: {topic.EnableBatchedOperations}");
            Console.WriteLine($"{nameof(topic.EnablePartitioning)}: {topic.EnablePartitioning}");
            Console.WriteLine($"{nameof(topic.MaxSizeInMB)}: {topic.MaxSizeInMB}");
            Console.WriteLine($"{nameof(topic.RequiresDuplicateDetection)}: {topic.RequiresDuplicateDetection}");
            Console.WriteLine($"{nameof(topic.Status)}: {topic.Status}");
            Console.WriteLine($"{nameof(topic.SupportOrdering)}: {topic.SupportOrdering}");
            Console.WriteLine();
        }
        static async Task GetQueue(string queueName)
        {
            if (await _managementClient.QueueExistsAsync(queueName))
            {
                QueueDescription queue = await _managementClient.GetQueueAsync(queueName);
                PrintQueueDescription(queue);
            }
        }
        static async Task CreateQueue(string queueName)
        {
            if (!await _managementClient.QueueExistsAsync(queueName))
            {
                QueueDescription queueDescription = new QueueDescription(queueName)
                {
                    EnablePartitioning = true,
                    DefaultMessageTimeToLive = TimeSpan.FromHours(1),
                    RequiresDuplicateDetection = true,
                    RequiresSession = true
                };
                var result = await _managementClient.CreateQueueAsync(queueDescription);
                if (result != null)
                {
                    Console.WriteLine($"{queueName} created successfully");
                }
            }
        }
        static async Task GetQueues()
        {
            IEnumerable<QueueDescription> queues = await _managementClient.GetQueuesAsync();
            foreach (QueueDescription queue in queues)
            {
                PrintQueueDescription(queue);
            }
        }

        static async Task GetTopic(string topicName)
        {
            if (await _managementClient.TopicExistsAsync(topicName))
            {
                TopicDescription topicDescription = await _managementClient.GetTopicAsync(topicName);
                if (topicDescription != null)
                {
                    PrintTopicDescription(topicDescription);
                }
            }
        }
        static async Task CreateTopic(string topicName)
        {
            if (!await _managementClient.TopicExistsAsync(topicName))
            {
                TopicDescription topicDescription = new TopicDescription(topicName)
                {
                    EnablePartitioning = true,
                    AutoDeleteOnIdle = TimeSpan.FromMinutes(5),
                    RequiresDuplicateDetection = true
                };
                topicDescription = await _managementClient.CreateTopicAsync(topicDescription);
                if (topicDescription != null)
                {
                    Console.WriteLine($"{topicName} created successfully\n");
                }
            }

        }
        static async Task GetTopics()
        {
            IEnumerable<TopicDescription> topics = await _managementClient.GetTopicsAsync();
            foreach (TopicDescription topic in topics)
            {
                PrintTopicDescription(topic);
            }
        }


        static async Task CreateSubscription(string topicName, string subscriptionName)
        {
            if (await _managementClient.TopicExistsAsync(topicName) &&
                !await _managementClient.SubscriptionExistsAsync(topicName, subscriptionName))
            {
                var result = await _managementClient.CreateSubscriptionAsync(new SubscriptionDescription(topicName, subscriptionName)
                {
                    AutoDeleteOnIdle = TimeSpan.FromMinutes(5),
                    EnableDeadLetteringOnMessageExpiration = true,
                    LockDuration = TimeSpan.FromSeconds(10)
                });
                if (result != null)
                {
                    Console.WriteLine($"{subscriptionName} was created on topic: {topicName}\n");
                }
            }
        }
        static void PrintSubscriptionDescription(SubscriptionDescription subscription)
        {
            Console.WriteLine($"{nameof(subscription.TopicPath)}: {subscription.TopicPath}");
            Console.WriteLine($"{nameof(subscription.SubscriptionName)}: {subscription.SubscriptionName}");
            Console.WriteLine($"{nameof(subscription.AutoDeleteOnIdle)}: {subscription.AutoDeleteOnIdle}");
            Console.WriteLine($"{nameof(subscription.DefaultMessageTimeToLive)}: {subscription.DefaultMessageTimeToLive}");
            Console.WriteLine($"{nameof(subscription.EnableBatchedOperations)}: {subscription.EnableBatchedOperations}");
            Console.WriteLine($"{nameof(subscription.EnableDeadLetteringOnFilterEvaluationExceptions)}: {subscription.EnableDeadLetteringOnFilterEvaluationExceptions}");
            Console.WriteLine($"{nameof(subscription.EnableDeadLetteringOnMessageExpiration)}: {subscription.EnableDeadLetteringOnMessageExpiration}");
            Console.WriteLine($"{nameof(subscription.LockDuration)}: {subscription.LockDuration}");
            Console.WriteLine($"{nameof(subscription.MaxDeliveryCount)}: {subscription.MaxDeliveryCount}");
            Console.WriteLine($"{nameof(subscription.RequiresSession)}: {subscription.RequiresSession}");
            Console.WriteLine($"{nameof(subscription.Status)}: {subscription.Status}");
            Console.WriteLine();
        }
        static async Task GetSubscriptions(string topicName)
        {

            if (await _managementClient.TopicExistsAsync(topicName))
            {
                IEnumerable<SubscriptionDescription> subscriptions =
                    await _managementClient.GetSubscriptionsAsync(topicName);
                foreach (SubscriptionDescription subscription in subscriptions)
                {
                    PrintSubscriptionDescription(subscription);
                }
            }
        }
        static void Main(string[] args)
        {
            //GetQueue("introqueue").Wait();
            //CreateQueue("queueone").Wait();
            GetQueues().Wait();


            //GetTopic("ChatTopic").Wait();
            //CreateTopic("topic1").Wait();
            //GetTopics().Wait();

            //CreateSubscription("ChatTopic", "raraanjani").Wait();
            //GetSubscriptions("ChatTopic").Wait();

            Console.ReadLine();
        }
    }
}
