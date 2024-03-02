using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatBot.Database.Models;

namespace ChatBot.Repositories.Interfaces
{
    public interface IComplaintRepository : IBaseRepository<Complaint>
    {
        Task<List<Complaint>> GetAllComplaintsByUserId(int customerId);
        Task<Complaint> GetSingleComplaint(int complaintId);
        Task<List<Complaint>> GetComplaintsByCategory(string category);
        Task<List<Complaint>> GetComplaintsByPlatform(string platform);
        Task<List<Complaint>> GetComplaintsByDateRange(DateTime startDate, DateTime endDate);
        Task<List<Complaint>> SearchComplaints(string searchTerm);
        Task AddComplaint(Complaint complaint);
    }
}
