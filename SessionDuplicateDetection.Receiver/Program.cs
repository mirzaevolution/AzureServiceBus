using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Newtonsoft.Json;
namespace SessionDuplicateDetection.Receiver
{
    public class MessagePayload
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Message { get; set; }
    }
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=LrtPyL+CWb/1FFtAVgZBpgySsdaEnSQCGjaCaScnphI=";
        private static string _queueName = "session_queue";
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
                    DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(10),
                    RequiresSession = true,
                    EnablePartitioning = true

                }).Wait();
            }
            _queueClient = new QueueClient(_connectionString, _queueName);
        }

        static void Main(string[] args)
        {
            _queueClient.RegisterSessionHandler(SessionHandler, new SessionHandlerOptions(ErrorSessionHandler)
            {
                AutoComplete = false,
                MessageWaitTimeout = TimeSpan.FromSeconds(5),
                MaxConcurrentSessions = 10,
                MaxAutoRenewDuration = TimeSpan.FromMinutes(5)
            });
            Console.WriteLine("Session handler registered");
            Console.ReadLine();
            _queueClient.CloseAsync().Wait();
            _managementClient.CloseAsync().Wait();
        }

        private static Task ErrorSessionHandler(ExceptionReceivedEventArgs error)
        {

            Console.WriteLine($"Error:\n{error.Exception?.Message}");
            return Task.CompletedTask;
        }

        private static async Task SessionHandler(IMessageSession session, Message message, CancellationToken cancellationToken)
        {
            string messageJson = Encoding.UTF8.GetString(message.Body);
            MessagePayload messagePayload = JsonConvert.DeserializeObject<MessagePayload>(messageJson);
            Console.WriteLine($"[{session.SessionId}] ID: {messagePayload.Id}, Message: {messagePayload.Message}");
            await session.CompleteAsync(message.SystemProperties.LockToken);
        }

    }
}
