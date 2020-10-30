using System;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System.Threading;
using System.Collections.Generic;
namespace Rewind.SessionDuplicateDetection.Queue.Receiver
{
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution-21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3hGIA7ykTK3Ryj2/dGnu9XcQvaGLwbeyvZe0gMcjH44=";
        private static string _queueName = "sessionduplicatedetection";

        private static QueueClient _queueClient;

        private static void Init()
        {
            try
            {
                ManagementClient managementClient = new ManagementClient(_connectionString);
                if (!managementClient.QueueExistsAsync(_queueName).Result)
                {
                    managementClient.CreateQueueAsync(new QueueDescription(_queueName)
                    {
                        AutoDeleteOnIdle = TimeSpan.FromHours(1),
                        EnablePartitioning = true,
                        RequiresDuplicateDetection = true,
                        DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(1),
                        RequiresSession = true
                    }).Wait();
                }
                _queueClient = new QueueClient(_connectionString, _queueName);
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
        private static void Register()
        {
            _queueClient.RegisterSessionHandler(SessionHandler, new SessionHandlerOptions(ErrorHandler)
            {
                AutoComplete = false,
                MaxConcurrentSessions = 2,
                MaxAutoRenewDuration = TimeSpan.FromMinutes(5)
            });
        }

        private static Task ErrorHandler(ExceptionReceivedEventArgs arg)
        {
            Console.WriteLine($"Error: {arg.Exception}");
            return Task.CompletedTask;
        }

        private static async Task SessionHandler(IMessageSession messageSession, Message message, CancellationToken token)
        {
            string messageId = message.MessageId;
            string sessionKey = message.SessionId;
            string content = Encoding.UTF8.GetString(message.Body);
            Console.WriteLine($"[{sessionKey}]-%{messageId}%: {content}");
            await messageSession.CompleteAsync(message.SystemProperties.LockToken);
        }

        static void Main(string[] args)
        {
            Register();
            Console.WriteLine("Press ENTER to quit");
            Console.ReadLine();
        }
    }
}
