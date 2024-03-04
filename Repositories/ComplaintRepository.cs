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
    public class ComplaintRepository : RepositoryBase<Complaint>, IComplaintRepository
    {
        private readonly BankDbContext _DbContext;

        public ComplaintRepository(BankDbContext context) : base(context)
        {
            _DbContext = context;
        }

        public async Task<List<Complaint>> GetAllComplaintsByComplaintNo(int accountId, string complaintNo)
        {
            try
            {
                return await _DbContext.Complaints
                .Where(c => c.AccountId == accountId && c.ComplaintNo == complaintNo).ToListAsync();
            }
            catch (Exception)
            {
                return new List<Complaint>();


            }
        }

        public async Task<Complaint> GetSingleComplaint(string complaintNo)
        {
            return await _DbContext.Complaints.FindAsync(complaintNo);
        }

        public async Task<List<Complaint>> GetComplaintsByCategory(string category)
        {
            return await _DbContext.Complaints
                .Where(c => c.Category == category)
                .ToListAsync();
        }

        public async Task<List<Complaint>> GetComplaintsByChannel(Channel channel)
        {
            return await _DbContext.Complaints
                .Where(c => c.Channel == channel)
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

        public async Task<List<Complaint>> GetComplaintsByStatus(Status complaintStatus)
        {
            return await _DbContext.Complaints
                .Where(c => c.ComplaintStatus == complaintStatus)
                .ToListAsync();
        }
    }
}
