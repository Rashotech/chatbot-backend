using System.ComponentModel.DataAnnotations.Schema;

namespace ChatBot.Database.Models
{
    public class Feedback : BaseEntity
    {
        public Feedback() { }

        public int Rating { get; set; }
        public string Review { get; set; }
    }
}
