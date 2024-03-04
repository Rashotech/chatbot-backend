using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatBot.Database.Models
{
	public class Transaction: BaseEntity
	{
        [Required]
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public Account Account { get; set; }

        [Required]
        public string TransactionReference { get; set; }

        [Required]
        public TransactionType TransactionType { get; set; }

        [Required]
        public TransactionDirection Direction { get; set; }

        [Required]
        public TransactionChannel Channel { get; set; }

        public string Currency { get; set; } = "NGN";

        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        [Required]
        public int Amount { get; set; }

        public string Narration { get; set; }

        public string RecipientAccountNumber { get; set; }

        public string RecipientBankCode { get; set; }

        public string RecipientName { get; set; }

        public string RecipientBankName { get; set; }

        public static explicit operator Transaction(List<Transaction> v)
        {
            throw new NotImplementedException();
        }
    }

    public enum TransactionDirection
    {
        Credit,
        Debit
    }

    public enum TransactionStatus
    {
        Pending,
        Successful,
        Failed,
        Reversed
    }

    public enum TransactionType
    {
        Deposit,
        Withdrawal,
        Transfer
    }

    public enum TransactionChannel
    {
        ATM,
        Branch,
        ChatBot,
        Mobile,
        USSD,
        Web,
        POS
    }
}

