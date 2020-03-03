using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Newtonsoft.Json;

namespace MultiThread.BatchSender
{
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=LrtPyL+CWb/1FFtAVgZBpgySsdaEnSQCGjaCaScnphI=";
        private static string _queueName = "payload_queue";
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
                    RequiresDuplicateDetection = true
                }).Wait();
            }
            _queueClient = new QueueClient(_connectionString, _queueName);
        }
        static async Task SendBatch(int number = 10)
        {
            List<Message> messages = new List<Message>();
            for (int i = 1; i <= number; i++)
            {
                DataPayload dataPayload = new DataPayload
                {
                    Title = $"Title - {i}",
                    Text = $"Payload - {i}"
                };
                string jsonPayload = JsonConvert.SerializeObject(dataPayload);
                byte[] bytesPayload = Encoding.UTF8.GetBytes(jsonPayload);

                Message message = new Message
                {
                    Body = bytesPayload,
                    ContentType = "application/json"
                };
                message.UserProperties.Add("unique_id", Guid.NewGuid());
                message.UserProperties.Add("sent_utc", DateTime.UtcNow);
                messages.Add(message);
            }
            Console.WriteLine($"Sending batch messages with total data: {number} messages");
            await _queueClient.SendAsync(messages);
            Console.WriteLine("Sent");
        }
        static void Main(string[] args)
        {
            SendBatch().Wait();
            Console.ReadLine();
            _queueClient.CloseAsync().Wait();

            _managementClient.CloseAsync().Wait();
        }
    }
}
