using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Rewind.AzFunc.Out.Profile.HttpTrigger
{
    public static class ProfileInvokerHttp
    {
        [FunctionName("ProfileInvoker")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [ServiceBus("azfuncprofile", Microsoft.Azure.WebJobs.ServiceBus.EntityType.Queue, Connection = "AzureServiceBusConnectionString")] IAsyncCollector<Profile> profileCollector,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            Profile profile = JsonConvert.DeserializeObject<Profile>(await req.ReadAsStringAsync());

            await profileCollector.AddAsync(profile);


            return new OkObjectResult(new { success = true, id = profile.Id });
        }
    }
}
