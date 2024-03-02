using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ChatBot.Database.Models;

namespace ChatBot.Dtos
{
    public class LogComplaintDto
    {
        [Required]
        public string Category { get; set; }

        [Required]
        public string Platform { get; set; }

        public DateTime Date { get; set; }

        [Required]
        public string Ref { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public int Amount { get; set; }
        
        [Required]
        public int ComplaintId { get; set; }

        
    }
}