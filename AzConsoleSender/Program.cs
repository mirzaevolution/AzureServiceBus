using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Newtonsoft.Json;

namespace AzConsoleSender
{
    class Program
    {
        private static string _topicName = "az_one_topic";
        private static string _connectionString = "Endpoint=sb://mirzaevolution21.servicebus.windows.net/;SharedAccessKeyName=sender;SharedAccessKey=GO1h386pXHO7f57F3HazLhlOpuJulbOqKnG/UkkmdIg=;";
        private static TopicClient _topicClient;
        static Program()
        {
            _topicClient = new TopicClient(_connectionString, _topicName);
        }
        static async Task Init()
        {
            List<Message> messages = new List<Message>();
            Random rand = new Random();
            Console.WriteLine("[*].......Preparing messages");
            for (int i = 1; i <= 5; i++)
            {
                var trxDetail = new TransactionDetail
                {
                    Id = Guid.NewGuid(),
                    AccountName = $"Acc-{rand.Next(1000, 10000)}-{rand.Next(1000, 10000)}-{rand.Next(1000, 10000)}-{rand.Next(1000, 10000)}",
                    AccountNumber = Guid.NewGuid().ToString(),
                    Amount = rand.Next(1000, 100000),
                    TargetAccountNumber = $"Acc-{rand.Next(1000, 10000)}-{rand.Next(1000, 10000)}-{rand.Next(1000, 10000)}-{rand.Next(1000, 10000)}",
                    TransactionDate = DateTime.Now
                };
                byte[] serializedBytes = Encoding.UTF8.GetBytes(
                        JsonConvert.SerializeObject(trxDetail)
                    );

                Message message = new Message(serializedBytes)
                {
                    MessageId = trxDetail.Id.ToString()
                };
                messages.Add(message);
            }
            Console.WriteLine("[*].......Sending messages");
            await _topicClient.SendAsync(messages);
            Console.WriteLine("[*].......Messages sent");
            Console.ReadLine();
            await _topicClient.CloseAsync();

        }
        static void Main(string[] args)
        {
            Init().Wait();
        }
    }
}
