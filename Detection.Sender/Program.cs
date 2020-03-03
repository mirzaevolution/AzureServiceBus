using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Newtonsoft.Json;


namespace Detection.Sender
{
    public class MessagePayload
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Message { get; set; }
    }
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=LrtPyL+CWb/1FFtAVgZBpgySsdaEnSQCGjaCaScnphI=";
        private static string _queueName = "detection_queue";
        private static ManagementClient _managementClient;
        private static QueueClient _queueClient;
        static Program()
        {
            _managementClient = new ManagementClient(_connectionString);
            if (!_managementClient.QueueExistsAsync(_queueName).Result)
            {
                _managementClient.CreateQueueAsync(new QueueDescription(_queueName)
                {
                    AutoDeleteOnIdle = TimeSpan.FromHours(1),
                    EnableBatchedOperations = true,
                    EnableDeadLetteringOnMessageExpiration = true,
                    LockDuration = TimeSpan.FromMinutes(1),
                    RequiresDuplicateDetection = true,
                    DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(10)
                }).Wait();
            }
            _queueClient = new QueueClient(_connectionString, _queueName);
        }
        static async Task Start()
        {
            List<MessagePayload> messagePayloads = new List<MessagePayload>();
            messagePayloads.Add(new MessagePayload
            {
                Id = "AABB",
                Message = "Message 1"
            });
            messagePayloads.Add(new MessagePayload
            {
                Id = "BBAA",
                Message = "Message 2"
            });
            messagePayloads.Add(new MessagePayload
            {
                Id = "CCDD",
                Message = "Message 3"
            });
            messagePayloads.Add(new MessagePayload
            {
                Id = "AABB",
                Message = "Message 4"
            });
            List<Message> messages = new List<Message>();
            foreach (var item in messagePayloads)
            {
                string messageJson = JsonConvert.SerializeObject(item);
                byte[] messageBytes = Encoding.UTF8.GetBytes(messageJson);

                messages.Add(new Message(messageBytes)
                {
                    MessageId = item.Id
                });
            }
            await _queueClient.SendAsync(messages);
            Console.WriteLine("Messages sent.");
        }
        static void Main(string[] args)
        {
            Start().Wait();
            Console.ReadLine();
            _queueClient.CloseAsync().Wait();
            _managementClient.CloseAsync().Wait();
        }
    }
}
