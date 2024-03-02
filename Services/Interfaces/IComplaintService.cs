using System;
using ChatBot.Database.Models;
using System.Threading.Tasks;
using ChatBot.Dtos;

namespace ChatBot.Services.Interfaces
{
    public interface IComplaintService
    {
        Task<Complaint> LogComplaintAsync(LogComplaintDto logComplaintDto);
        
    }
}

