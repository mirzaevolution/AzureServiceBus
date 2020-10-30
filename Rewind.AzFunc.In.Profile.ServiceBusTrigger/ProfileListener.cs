using System;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Rewind.AzFunc.In.Profile.ServiceBusTrigger
{
    public static class ProfileListener
    {
        [FunctionName("ProfileListenerFunc")]
        public static void Run([ServiceBusTrigger("azfuncprofiletopic", "azfunc", Connection = "AzureServiceBusConnectionString")]
            Message rawMessage,
            ILogger log)
        {
            string jsonData = Encoding.UTF8.GetString(rawMessage.Body);
            Profile profile = JsonConvert.DeserializeObject<Profile>(jsonData);
            log.LogInformation("'ProfileListenerFunc' processing the message.....");
            log.LogInformation(profile.ToString());
        }
    }
}
