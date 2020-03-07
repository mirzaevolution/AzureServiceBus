using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
namespace ReqV1.Client
{
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=LrtPyL+CWb/1FFtAVgZBpgySsdaEnSQCGjaCaScnphI=";

        private static string _queueClientName = "reqresv1_client_queue";
        private static string _queueServerName = "reqresv1_server_queue";

        private static ManagementClient _managementClient;
        private static QueueClient _queueClient;

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
        }
        static Program()
        {
            Init();
        }
        static async Task SendMessage(string input, string sessionId)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(input);
            Message message = new Message(messageBytes)
            {
                MessageId = Guid.NewGuid().ToString(),
                ReplyToSessionId = sessionId //this used to reply to the specified session id
            };
            await _queueClient.SendAsync(message);
            Console.WriteLine($"[{DateTime.Now}] Message sent to the server.");

        }
        static async Task ReceiveMessage(string sessionId)
        {
            try
            {
                SessionClient sessionClient = new SessionClient(_connectionString, _queueServerName);
                IMessageSession messageSession = await sessionClient.AcceptMessageSessionAsync(sessionId);
                Message message = await messageSession.ReceiveAsync();
                if (message != null)
                {
                    string messageText = Encoding.UTF8.GetString(message.Body);
                    Console.WriteLine($"[{DateTime.Now}] Reply from server: {messageText}");
                }
                await messageSession.CloseAsync();
                await sessionClient.CloseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[x] Error: {ex.Message}");
            }

        }
        static async Task DoLoopRequests()
        {
            try
            {
                string input = string.Empty;
                Console.WriteLine($"** Starting ReqResV1 Client **");
                Console.WriteLine("Type [exit] to close the application\n");
                while (true)
                {
                    string sessionId = Guid.NewGuid().ToString();

                    Console.Write($"[{sessionId}]> ");
                    input = Console.ReadLine().Trim();
                    if (string.IsNullOrEmpty(input))
                    {
                        continue;
                    }
                    if (input.Equals("exit", StringComparison.InvariantCultureIgnoreCase))
                    {
                        break;
                    }
                    await SendMessage(input, sessionId);
                    await ReceiveMessage(sessionId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static void Main(string[] args)
        {
            DoLoopRequests().Wait();
        }
    }
}
