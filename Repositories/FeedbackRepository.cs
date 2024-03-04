using ChatBot.Database;
using ChatBot.Database.Models;
using ChatBot.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatBot.Repositories
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly BankDbContext _dbContext;

        public FeedbackRepository(BankDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Feedback> GetFeedbackByIdAsync(int id)
        {
            return await _dbContext.Feedbacks.FindAsync(id);
        }

        public async Task<IEnumerable<Feedback>> GetAllFeedbackAsync()
        {
            return await _dbContext.Feedbacks.ToListAsync();
        }

        public async Task<Feedback> SaveFeedbackAsync(Feedback feedback)
        {
            _dbContext.Feedbacks.Add(feedback);
            await _dbContext.SaveChangesAsync();
            return feedback;
        }
    }
}
