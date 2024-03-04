using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatBot.Database.Models
{
    public class Complaint : BaseEntity
    {
        [Required]
        public string ComplaintNo { get; set; }

        [Required]
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]

        public Account Account { get; set; }


        [Required]
        public int Amount { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Category { get; set; }
        [Required]
        public string TransactionRef { get; set; }

        [Required]
        
        public Channel Channel { get; set; }

        public string Description { get; set; }

        public Status ComplaintStatus { get; set; }

        public static explicit operator Complaint(List<Complaint> v)
        {
            throw new NotImplementedException();
        }
    }

    public enum Channel
    {
        ATM,
        Branch,
        ChatBot,
        Mobile,
        USSD,
        Web,
        POS
    }

    public enum Status
    {
        Pending,
        Resolved
    }
}

