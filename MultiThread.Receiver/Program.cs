using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Newtonsoft.Json;


namespace MultiThread.Receiver
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
        static void Initialize(int threadNumber = 1)
        {
            _queueClient.RegisterMessageHandler(MessageHandler, new MessageHandlerOptions(ErrorHandler)
            {
                AutoComplete = false,
                MaxAutoRenewDuration = TimeSpan.FromMinutes(10),
                MaxConcurrentCalls = threadNumber
            });
            Console.WriteLine("** Queue Message Handler Registered **");

        }

        private static Task ErrorHandler(ExceptionReceivedEventArgs error)
        {
            Console.WriteLine($"Error:\n{error.Exception?.Message}");
            return Task.CompletedTask;
        }

        private static async Task MessageHandler(Message message, CancellationToken cancellationToken)
        {
            ProcessMessage(message);
            await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        private static void ProcessMessage(Message message)
        {
            Guid uniqueId = Guid.Parse(message.UserProperties["unique_id"].ToString());
            DateTime sentUtc = DateTime.Parse(message.UserProperties["sent_utc"].ToString());

            Console.WriteLine($"\n[*] Processing item id {uniqueId} [sent_utc: {sentUtc}]");
            Thread.Sleep(2000);
            string body = Encoding.UTF8.GetString(message.Body);
            DataPayload payload = JsonConvert.DeserializeObject<DataPayload>(body);
            Console.WriteLine($"[#] Data id {uniqueId}.\n[#] Title:{payload.Title}\n[#] Message: {payload.Text}\n");

        }

        static void Main(string[] args)
        {
            //Initialize();
            Initialize(20);
            Console.ReadLine();
            _queueClient.CloseAsync().Wait();
            _managementClient.CloseAsync().Wait();
        }
    }
}
