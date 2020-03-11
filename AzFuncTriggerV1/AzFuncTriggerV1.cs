using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AzFuncTriggerV1
{
    public static class AzFuncTriggerV1
    {
        [FunctionName("trx_queue_listener")]
        public static void Run([ServiceBusTrigger("trx_queue", Connection = "ServiceBusConnectionString")]
            TransactionDetail transactionDetail,
            string messageId,
            ILogger log)
        {
            if (transactionDetail != null)
            {
                log.LogInformation($"Receiving queue with message id: {messageId}");
                log.LogInformation(transactionDetail.ToString());
                
            }
            else
            {
                log.LogInformation("No transcation received");
            }
        }
    }
}
