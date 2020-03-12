using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AzFuncTopicsTrigger
{
    public static class TopicListener
    {
        [FunctionName("TopicListener")]
        public static void Run([ServiceBusTrigger("az_one_topic", "az_func_sub", Connection = "ServiceBusConnectionString")]
            TransactionDetail transactionDetail,
            string messageId,
            ILogger log)
        {
            if (transactionDetail != null)
            {
                log.LogInformation($"\nProcessing information with message id: {messageId}");
                log.LogInformation(transactionDetail.ToString());
            }
            else
            {
                log.LogInformation("Payload information is empty");
            }
        }
    }
}
