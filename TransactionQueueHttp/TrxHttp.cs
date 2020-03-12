using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TransactionQueueHttp
{
    public static class TrxHttp
    {
        [FunctionName("TransactionInvoker")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [ServiceBus("trx_queue",Connection = "ServiceBusConnectionString")]
            IAsyncCollector<TransactionDetail> transcationCollector,
            ILogger log)
        {
            try
            {

                using (StreamReader reader = new StreamReader(req.Body))
                {
                    string jsonData = await reader.ReadToEndAsync();
                    if (string.IsNullOrEmpty(jsonData))
                    {
                        return new StatusCodeResult(400);
                    }
                    TransactionDetail transactionDetail = JsonConvert.DeserializeObject<TransactionDetail>(jsonData);
                    if (transactionDetail != null)
                    {
                        await transcationCollector.AddAsync(transactionDetail);
                    }
                    return new OkResult();
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                return new StatusCodeResult(500);
            }
        }
    }
}
