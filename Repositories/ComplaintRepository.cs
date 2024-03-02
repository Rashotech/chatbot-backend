using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatBot.Database;
using ChatBot.Database.Models;
using ChatBot.Repositories.Interfaces;
using Microsoft.Bot.Schema.Teams;
using Microsoft.EntityFrameworkCore;

namespace ChatBot.Repositories
{
    public class ComplaintRepository : RepositoryBase<Complaint>,  IComplaintRepository
    {
        private readonly BankDbContext _DbContext;

        public ComplaintRepository(BankDbContext context) : base(context)
        {
            _DbContext = context;
        }

        public async Task AddComplaint(Complaint complaint)
        {
            await _DbContext.Complaints.AddAsync(complaint);
            await _DbContext.SaveChangesAsync();
        }

        public async Task<List<Complaint>> GetAllComplaintsByUserId(int complaintId)
        {
            return await _DbContext.Complaints
                .Where(c => c.ComplaintId == complaintId)
                .ToListAsync();
        }

        public async Task<Complaint> GetSingleComplaint(int complaintId)
        {
            return await _DbContext.Complaints.FindAsync(complaintId);
        }

        public async Task<List<Complaint>> GetComplaintsByCategory(string category)
        {
            return await _DbContext.Complaints
                .Where(c => c.Category == category)
                .ToListAsync();
        }

        public async Task<List<Complaint>> GetComplaintsByPlatform(string platform)
        {
            return await _DbContext.Complaints
                .Where(c => c.Platform == platform)
                .ToListAsync();
        }

        public async Task<List<Complaint>> GetComplaintsByDateRange(DateTime startDate, DateTime endDate)
        {
            return await _DbContext.Complaints
                .Where(c => c.Date >= startDate && c.Date <= endDate)
                .ToListAsync();
        }

        public async Task<List<Complaint>> SearchComplaints(string searchTerm)
        {
            return await _DbContext.Complaints
                .Where(c => c.Description.Contains(searchTerm))
                .ToListAsync();
        }
    }
}
