using ChatBot.Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatBot.Repositories.Interfaces
{
    public interface IFeedbackRepository
    {
        Task<Feedback> GetFeedbackByIdAsync(int id);
        Task<IEnumerable<Feedback>> GetAllFeedbackAsync();
        Task<Feedback> SaveFeedbackAsync(Feedback feedback);
    }
}
