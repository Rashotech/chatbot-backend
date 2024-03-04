using ChatBot.Database.Models;
using ChatBot.Dtos;
using ChatBot.Repositories.Interfaces;
using ChatBot.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatBot.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _repository;

        public FeedbackService(IFeedbackRepository repository)
        {
            _repository = repository;
        }

        public async Task<Feedback> GetFeedbackByIdAsync(int id)
        {
            return await _repository.GetFeedbackByIdAsync(id);
        }

        public async Task<IEnumerable<Feedback>> GetAllFeedbackAsync()
        {
            return await _repository.GetAllFeedbackAsync();
        }


        public async Task<Feedback> SaveFeedbackAsync(SaveFeedbackDto feedbackDto)
        {
            var feedback = new Feedback
            {
                Rating = feedbackDto.Rating,
                Review = feedbackDto.Review
            };

            return await _repository.SaveFeedbackAsync(feedback);
        }
    }
}
