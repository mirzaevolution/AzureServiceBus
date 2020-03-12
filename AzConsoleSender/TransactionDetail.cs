using System;
using System.Collections.Generic;
using System.Text;

namespace AzConsoleSender
{
    public class TransactionDetail
    {
        public Guid Id { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string TargetAccountNumber { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
