using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Newtonsoft.Json;

namespace Rewind.AzFunc.In.Profile.Listener
{
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution-21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3hGIA7ykTK3Ryj2/dGnu9XcQvaGLwbeyvZe0gMcjH44=";
        private static string _queueName = "azfuncprofile";
        private static QueueClient _queueClient;
        private static ManagementClient _managementClient;
        static void Init()
        {
            try
            {
                _managementClient = new ManagementClient(_connectionString);
                if (!_managementClient.QueueExistsAsync(_queueName).Result)
                {
                    _managementClient.CreateQueueAsync(new QueueDescription(_queueName)
                    {
                        AutoDeleteOnIdle = TimeSpan.FromHours(1),
                        EnableDeadLetteringOnMessageExpiration = true
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
        static void Listen()
        {
            _queueClient.RegisterMessageHandler(MessageHandler, new MessageHandlerOptions(ErrorHandler)
            {
                AutoComplete = false,
                MaxConcurrentCalls = 10
            });
        }

        private static Task ErrorHandler(ExceptionReceivedEventArgs arg)
        {
            Console.WriteLine($"Error: {arg.Exception.Message}");
            return Task.CompletedTask;
        }

        private static async Task MessageHandler(Message message, CancellationToken token)
        {
            string jsonObj = Encoding.UTF8.GetString(message.Body);
            Profile profile = JsonConvert.DeserializeObject<Profile>(jsonObj);
            Console.WriteLine($"[*] {profile.ToString()}");
            await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        static void Main(string[] args)
        {
            Listen();
            Console.WriteLine("Press ENTER to quit");
            Console.ReadLine();
        }
    }
}
