using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
namespace Intro.Sender
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
        static async Task SendMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                try
                {

                    Message messageContent = new Message(Encoding.UTF8.GetBytes(message));
                    await _queueClient.SendAsync(messageContent);
                    Console.WriteLine($"`{message}` has been sent");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"`{message}` failed to send\n");
                    Console.WriteLine(ex.ToString());
                }
            }
        }
        static void Main(string[] args)
        {

            while (true)
            {
                string message = string.Empty;
                Console.Write("Enter message: ");
                message = Console.ReadLine();
                if (message.Equals("exit", StringComparison.InvariantCultureIgnoreCase))
                {
                    break;
                }
                SendMessage(message).Wait();

            }
            Console.WriteLine("Press [ENTER] to exit");
            Console.ReadLine();
            CloseQueueClient().Wait();
        }
    }
}
