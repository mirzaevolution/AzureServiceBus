using System;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
namespace Rewind.MultiThread.Queue.Sender
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
        static async Task Send(string input)
        {
            try
            {
                Message message = new Message(Encoding.UTF8.GetBytes(input))
                {
                    MessageId = Guid.NewGuid().ToString()
                };
                await _queueClient.SendAsync(message);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static void Main(string[] args)
        {
            for (int i = 1; i <= 10; i++)
            {
                Send($"Message {i}").Wait();
            }
            Console.WriteLine("Message sent. Press enter to quit");
            Console.ReadLine();
            _queueClient.CloseAsync().Wait();

        }
    }
}
