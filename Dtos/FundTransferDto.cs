using System;
using ChatBot.Database.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatBot.Dtos
{
	public class FundTransferDto
	{
        [Required]
        public int AccountId { get; set; }

        public TransactionChannel Channel { get; set; } = TransactionChannel.ChatBot;

        [Required]
        public decimal Amount { get; set; }

        public string Narration { get; set; } = "";

        [Required]
        public string RecipientAccountNumber { get; set; }

        [Required]
        public string RecipientBankCode { get; set; }

        [Required]
        public string RecipientName { get; set; }

        [Required]
        public string RecipientBankName { get; set; }
    }
}

