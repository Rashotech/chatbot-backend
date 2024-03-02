using System;
using ChatBot.Database.Models;
using System.Threading.Tasks;
using ChatBot.Dtos;

namespace ChatBot.Services.Interfaces
{
    public interface IComplaintService
    {
        void AddComplaintAsync(Complaint complaint);
        Task<Complaint> LogComplaintAsync(LogComplaintDto logComplaintDto);
        
    }
}

