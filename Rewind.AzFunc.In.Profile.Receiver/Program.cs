using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Newtonsoft.Json;
namespace Rewind.AzFunc.In.Profile.Receiver
{
    class Program
    {
        private static string _connectionString = "Endpoint=sb://mirzaevolution-21.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3hGIA7ykTK3Ryj2/dGnu9XcQvaGLwbeyvZe0gMcjH44=";
        private static string _topicName = "azfuncprofiletopic";
        private static SubscriptionClient _subscriptionClient;

        static Program()
        {
            _subscriptionClient = new SubscriptionClient(_connectionString, _topicName, "profilesubscription");

        }

        static void Register()
        {
            _subscriptionClient.RegisterMessageHandler(MessageHandler, new MessageHandlerOptions(ErrorHandler)
            {
                AutoComplete = false
            });
        }

        private static Task ErrorHandler(ExceptionReceivedEventArgs arg)
        {
            return Task.CompletedTask;
        }

        private static async Task MessageHandler(Message message, CancellationToken token)
        {
            string text = Encoding.UTF8.GetString(message.Body);
            Profile profile = JsonConvert.DeserializeObject<Profile>(text);
            Console.WriteLine(profile.ToString());
            await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        static void Main(string[] args)
        {
            Register();
            Console.WriteLine("Press ENTER to quit");
            Console.ReadLine();
        }
    }
}
