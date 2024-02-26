
using System;
namespace ChatBot.Dtos
{
	public class TransactionDto
	{
        public int AccountId { get; set; }
        public string TransactionReference { get; set; }
        public string TransactionType { get; set; }
        public string Direction { get; set; }
        public string Channel { get; set; }
        public string Status { get; set; }
        public int Amount { get; set; }
        public string Narration { get; set; }
        public string RecipientAccountNumber { get; set; }
        public string RecipientBankCode { get; set; }
        public string RecipientName { get; set; }
        public string RecipientBankName { get; set; }
    }
}

