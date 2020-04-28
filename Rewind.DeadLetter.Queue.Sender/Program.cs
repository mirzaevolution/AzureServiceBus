using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Azure.ServiceBus.Core;

namespace Rewind.DeadLetter.Queue.Sender
{
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution-21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3hGIA7ykTK3Ryj2/dGnu9XcQvaGLwbeyvZe0gMcjH44=";
        private static string _queueName = "corequeue1";
        private static QueueClient _queueClient;
        private static QueueClient _queueDeadLetterClient;
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
                string deadLetterPath = EntityNameHelper.FormatDeadLetterPath(_queueName);
                _queueDeadLetterClient = new QueueClient(_connectionString, deadLetterPath);


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
        static void Send()
        {
            while (true)
            {

                Console.Write(">> ");
                string input = Console.ReadLine();
                if (input.Equals("exit", StringComparison.InvariantCultureIgnoreCase))
                {
                    break;
                }
                Message message = new Message(Encoding.UTF8.GetBytes(input));
                _queueClient.SendAsync(message).Wait();
                Console.WriteLine("Message sent.");
            }
        }
        static void RegisterDeadLetterPath()
        {
            _queueDeadLetterClient.RegisterMessageHandler(DeadLetterMessageHandler, new MessageHandlerOptions(ErrorHandler)
            {
                AutoComplete = true
            });
        }

        private static Task ErrorHandler(ExceptionReceivedEventArgs context)
        {
            Console.WriteLine($"Error: {context.Exception.Message}");
            return Task.CompletedTask;

        }

        private static Task DeadLetterMessageHandler(Message message, CancellationToken cancellationToken)
        {
            string messageText = Encoding.UTF8.GetString(message.Body);
            Console.WriteLine($"[!] DeadLetterMessage: {messageText}");
            foreach (var errorDict in message.UserProperties)
            {
                Console.WriteLine($"[!] {errorDict.Key}: {errorDict.Value}");
            }
            Console.WriteLine();
            return Task.CompletedTask;
        }

        static void Main(string[] args)
        {
            RegisterDeadLetterPath();
            Send();
            Console.WriteLine("Press ENTER to quit");
            Console.ReadLine();
        }
    }
}
