using ChatBot.Database.Models;
using ChatBot.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatBot.Services.Interfaces
{
    public interface IFeedbackService
    {
        Task<Feedback> GetFeedbackByIdAsync(int id);
        Task<IEnumerable<Feedback>> GetAllFeedbackAsync();
        Task<Feedback> SaveFeedbackAsync(SaveFeedbackDto feedbackDto);
    }
}
