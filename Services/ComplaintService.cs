using System;
using System.Threading.Tasks;
using ChatBot.Database.Models;
using ChatBot.Repositories.Interfaces;
using ChatBot.Services.Interfaces;
using ChatBot.Dtos;
using ChatBot.Exceptions;

namespace ChatBot.Services
{
    public class ComplaintService : IComplaintService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ComplaintService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Complaint> LogComplaintAsync(LogComplaintDto logComplaintDto)
        {
            try
            {
                var guid = Guid.NewGuid();
                var hash = guid.GetHashCode();
                var complaintNo = $"comp-{Math.Abs(hash)}";
                Status complaintStatus = Status.Pending;


                var complaint = new Complaint
                {
                    ComplaintNo = complaintNo,
                    TransactionRef = logComplaintDto.TransactionRef,
                    Amount = logComplaintDto.Amount,
                    Date = logComplaintDto.Date,
                    Category = logComplaintDto.Category,
                    Channel = logComplaintDto.Channel,
                    Description = logComplaintDto.Description,
                    AccountId = logComplaintDto.AccountId,
                    ComplaintStatus = complaintStatus
                };

                _unitOfWork.Complaints.Add(complaint);
                await _unitOfWork.CommitAsync();

                return complaint;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to log complaint, {ex.Message}", ex);
            }
        }
    }
}
