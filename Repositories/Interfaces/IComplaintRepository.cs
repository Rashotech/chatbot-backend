using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatBot.Database.Models;

namespace ChatBot.Repositories.Interfaces
{
    public interface IComplaintRepository : IBaseRepository<Complaint>
    {
        Task<List<Complaint>> GetAllComplaintsByComplaintNo(string complaintNo);
        Task<Complaint> GetSingleComplaint(string complaintNo);
        Task<List<Complaint>> GetComplaintsByCategory(string category);
        Task<List<Complaint>> GetComplaintsByChannel(Channel channel);
        Task<List<Complaint>> GetComplaintsByDateRange(DateTime startDate, DateTime endDate);
        Task<List<Complaint>> GetComplaintsByStatus(Status complaintStatus);
        Task<List<Complaint>> SearchComplaints(string searchTerm);
    }
}
