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

namespace TransactionQueue.Sender
{
    class Program
    {

        private static string _queueName = "trx_queue";
        private static string _connectionString = "Endpoint=sb://mirzaevolution21.servicebus.windows.net/;SharedAccessKeyName=trx_queue_root;SharedAccessKey=TEefcXSgB0xfaLJTd28vo8w+PZhqxVTR04incCSD0G8=";
        private static QueueClient _queueClient;
        static Program()
        {
            _queueClient = new QueueClient(_connectionString, _queueName);
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
            await _queueClient.SendAsync(messages);
            Console.WriteLine("[*].......Messages sent");
            Console.ReadLine();
            await _queueClient.CloseAsync();

        }
        static void Main(string[] args)
        {
            Init().Wait();
        }
    }
}
