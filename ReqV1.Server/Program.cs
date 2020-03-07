using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

namespace ReqV1.Server
{
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=LrtPyL+CWb/1FFtAVgZBpgySsdaEnSQCGjaCaScnphI=";

        private static string _queueClientName = "reqresv1_client_queue";
        private static string _queueServerName = "reqresv1_server_queue";

        private static ManagementClient _managementClient;
        private static QueueClient _queueClient;
        private static QueueClient _queueServer;
        static void Init()
        {
            _managementClient = new ManagementClient(_connectionString);
            if (!_managementClient.QueueExistsAsync(_queueClientName).Result)
            {
                _managementClient.CreateQueueAsync(new QueueDescription(_queueClientName)
                {
                    AutoDeleteOnIdle = TimeSpan.FromHours(1)
                }).GetAwaiter().GetResult();
            }
            if (!_managementClient.QueueExistsAsync(_queueServerName).Result)
            {
                _managementClient.CreateQueueAsync(new QueueDescription(_queueServerName)
                {
                    AutoDeleteOnIdle = TimeSpan.FromHours(1),
                    RequiresSession = true
                }).GetAwaiter().GetResult();
            }
            _queueClient = new QueueClient(_connectionString, _queueClientName);
            _queueServer = new QueueClient(_connectionString, _queueServerName);
        }
        static Program()
        {
            Init();
        }
        static async Task DoLoopRequestResponse()
        {
            Console.WriteLine($"** Starting ReqResV1 Server **");

            _queueClient.RegisterMessageHandler(ClientMessageHandler, new MessageHandlerOptions(ClientErrorHandler)
            {
                AutoComplete = false,
                MaxConcurrentCalls = 5,
                MaxAutoRenewDuration = TimeSpan.FromMinutes(10)
            });
            Console.WriteLine("Client Handler registered successfully");
            Console.WriteLine("Press [ENTER] to close the application");
            Console.ReadLine();
            await _queueClient.CloseAsync();
            await _queueServer.CloseAsync();
        }

        private static Task ClientErrorHandler(ExceptionReceivedEventArgs error)
        {
            Console.WriteLine("[!] Error occured:");
            Console.WriteLine($"[!] ClientId: {error.ExceptionReceivedContext.ClientId}");
            Console.WriteLine($"[!] Message: {error.Exception.Message}");
            return Task.CompletedTask;
        }

        private static async Task ClientMessageHandler(Message message, CancellationToken cancellationToken)
        {
            string decodedMessage = Encoding.UTF8.GetString(message.Body);
            string sessionId = message.ReplyToSessionId;
            Console.WriteLine($"[{DateTime.Now}] {sessionId}: {decodedMessage}");
            string reply = $"Ping back `{decodedMessage}`";
            await _queueServer.SendAsync(new Message
            {
                SessionId = sessionId,
                Body = Encoding.UTF8.GetBytes(reply),
                MessageId = Guid.NewGuid().ToString()
            });
            Console.WriteLine($"[{DateTime.Now}] Response was sent to {sessionId}");
            await _queueClient.CompleteAsync(message.SystemProperties.LockToken);

        }

        static void Main(string[] args)
        {
            DoLoopRequestResponse().Wait();
        }
    }
}
