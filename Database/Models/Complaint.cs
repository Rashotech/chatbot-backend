using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChatBot.Database.Models
{
    public class Complaint : BaseEntity
    {
        [Required]
        [StringLength(20, ErrorMessage = "Complaint Id cannot exceed 20 characters")]
        public int ComplaintId { get; set; }
        [ForeignKey("ComplaintId")]

        public Account Account { get; set; }

        [Required]
        [StringLength(30, ErrorMessage = "Ref cannot exceed 30 characters")]
        public string Ref { get; set; }

        [Required]
        [StringLength(30, ErrorMessage = "Amount cannot exceed 30 characters")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Account number must be numeric")]
        public int Amount { get; set; }

        [StringLength(30, ErrorMessage = "Date cannot exceed 50 characters")]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(30, ErrorMessage = "Category cannot exceed 30 characters")]
        public string Category { get; set; }

        [Required]
        [StringLength(30, ErrorMessage = "Platform cannot exceed 30 characters")]
        
        public string Platform { get; set; }

        [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters")]
        public string Description { get; set; }

    }
}

