using System;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System.Threading;

namespace Rewind.MultiThread.Queue.Receiver
{
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution-21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3hGIA7ykTK3Ryj2/dGnu9XcQvaGLwbeyvZe0gMcjH44=";
        private static string _queueName = "rewindmultithreading";
        private static QueueClient _queueClient;
        private static ManagementClient _managementClient;
        static Program()
        {
            Init();
        }
        static void Init()
        {
            try
            {
                _managementClient = new ManagementClient(_connectionString);
                if (!_managementClient.QueueExistsAsync(_queueName).Result)
                {
                    _managementClient.CreateQueueAsync(new QueueDescription(_queueName)
                    {
                        EnableBatchedOperations = true,
                        AutoDeleteOnIdle = TimeSpan.FromHours(1)
                    }).Wait();

                }
                _queueClient = new QueueClient(_connectionString, _queueName);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
        static async Task Listen()
        {
            try
            {
                _queueClient.RegisterMessageHandler(MessageHandler, new MessageHandlerOptions(ErrorHandler)
                {
                    AutoComplete = false,
                    MaxConcurrentCalls = 100,
                    MaxAutoRenewDuration = TimeSpan.FromMinutes(10)
                });
                Console.WriteLine("** Handlers Registered ***");
                Console.WriteLine("Press ENTER to quit");
                Console.ReadLine();
                await _queueClient.CloseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static Task ErrorHandler(ExceptionReceivedEventArgs arg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR");
            Console.WriteLine(arg.Exception.ToString());
            return Task.CompletedTask;
        }

        private static Task MessageHandler(Message message, CancellationToken token)
        {
            try
            {
                Thread.Sleep(3000);
                string messageText = Encoding.UTF8.GetString(message.Body);
                Console.WriteLine($"[*] {messageText}");
                _queueClient.CompleteAsync(message.SystemProperties.LockToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return Task.CompletedTask;
        }

        static void Main(string[] args)
        {
            Listen().Wait();
        }
    }
}
