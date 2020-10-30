using System;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzFuncListener
{
    public static class QueueListener
    {
        [FunctionName("corequeue0_listener")]
        public static void Run([ServiceBusTrigger("corequeue0", Connection = "AzureServiceBusConnectionString")] Message message, ILogger log)
        {
            string jsonMessage = Encoding.UTF8.GetString(message.Body);
            Console.WriteLine($"Parsing: {jsonMessage}");
            try
            {

                Profile profile = JsonConvert.DeserializeObject<Profile>(jsonMessage);

                Console.WriteLine($"Parsed: {profile.ToString()}");
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Failed: {ex.Message.ToString()}");
            }
        }
    }
}
