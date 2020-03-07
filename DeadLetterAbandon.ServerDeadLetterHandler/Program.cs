using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
namespace DeadLetterAbandon.ServerDeadLetterHandler
{
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=LrtPyL+CWb/1FFtAVgZBpgySsdaEnSQCGjaCaScnphI=";
        private static string _queueName = "corequeue";
        private static QueueClient _queueClient;
        static Program()
        {
            string deadLetterQueueName = EntityNameHelper.FormatDeadLetterPath(_queueName);
            _queueClient = new QueueClient(_connectionString, deadLetterQueueName);
        }
        static void InitDeadLetterMessage()
        {
            _queueClient.RegisterMessageHandler(DeadLetterMessageHandler, new MessageHandlerOptions(ErrorHandler));

        }

        private static Task ErrorHandler(ExceptionReceivedEventArgs error)
        {
            Console.WriteLine($"Error: {error.Exception.Message}");
            return Task.CompletedTask;
        }

        private static Task DeadLetterMessageHandler(Message message, CancellationToken cancellationToken)
        {
            string messageText = string.Empty;
            if (message.Body != null)
            {
                messageText = Encoding.UTF8.GetString(message.Body);
            }
            if (!string.IsNullOrEmpty(messageText))
            {
                Console.WriteLine($"Dead letter message: {messageText}");
            }
            foreach (var item in message.UserProperties)
            {
                Console.WriteLine($"{item.Key}: {item.Value}");
            }
            return Task.CompletedTask;
        }

        static void Main(string[] args)
        {
            InitDeadLetterMessage();
            Console.ReadLine();
        }
    }
}
