using System;

namespace TransactionQueue.Listener
{
    public class TransactionDetail
    {
        public Guid Id { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string TargetAccountNumber { get; set; }
        public DateTime TransactionDate { get; set; }

        public override string ToString()
        {
            return $"\nID: {Id},\nAccountName: {AccountName},\nAccountNumber: {AccountNumber},\nAmount: ${Amount},\nTargetAccountNumber: {TargetAccountNumber},\nTransactionDate: {TransactionDate}";
        }
    }
}
