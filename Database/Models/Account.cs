using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChatBot.Database.Models
{
	public class Account: BaseEntity
	{
        [Required]
        [StringLength(10)]
        public string AccountNumber { get; set; }

        [Required]
        public AccountType AccountType { get; set; }

        public AccountStatus AccountStatus { get; set; } = AccountStatus.Active;

        [Required]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        public string Currency { get; set; } = "NGN";

        public int Balance { get; set; } = 0;

        [JsonIgnore]
        public ICollection<Transaction> Transactions { get; set; }
    }

    public enum AccountType
    {
        Savings,
        Current
    }

    public enum AccountStatus
    {
        Active,
        Dormant,
        Frozen
    }
}

