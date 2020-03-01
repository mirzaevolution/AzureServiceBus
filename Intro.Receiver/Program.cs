using Microsoft.Azure.ServiceBus;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Intro.Receiver
{
    class Program
    {
        private readonly static string _connectionString = "Endpoint=sb://mirzaevolution21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=LrtPyL+CWb/1FFtAVgZBpgySsdaEnSQCGjaCaScnphI=";
        private readonly static string _queueName = "introqueue";
        private static QueueClient _queueClient;

        static Program()
        {
            _queueClient = new QueueClient(_connectionString, _queueName);
        }
        static async Task CloseQueueClient()
        {
            if (_queueClient != null && _queueClient.OwnsConnection)
            {
                await _queueClient.CloseAsync();
            }
        }
        static void RegisterHandler()
        {
            try
            {
                _queueClient.RegisterMessageHandler(MessageHandler, ErrorHandler);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static Task ErrorHandler(ExceptionReceivedEventArgs error)
        {
            Console.WriteLine($"Error:\n{error.Exception?.Message}");
            return Task.CompletedTask;
        }
        private static Task MessageHandler(Message message, CancellationToken cancellationToken)
        {
            string messageContent = Encoding.UTF8.GetString(message.Body);
            Console.WriteLine($"[{DateTime.Now}] {messageContent}");
            return Task.CompletedTask;
        }

        static void Main(string[] args)
        {
            RegisterHandler();
            Console.ReadLine();
            CloseQueueClient().Wait();
        }
    }
}
