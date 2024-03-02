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

        public void Add(Complaint complaint)
        {
            throw new NotImplementedException();
        }

        public void AddComplaintAsync(Complaint complaint)
        {
            throw new NotImplementedException();
        }

        public async Task<Complaint> LogComplaintAsync(LogComplaintDto logComplaintDto)
        {
            try
            {
                var guid = Guid.NewGuid();
                var hash = guid.GetHashCode();
                var complaintId = Math.Abs(hash);


                var complaint = new Complaint
                {
                    ComplaintId = complaintId,
                    Ref = logComplaintDto.Ref,
                    Amount = logComplaintDto.Amount,
                    Date = logComplaintDto.Date,
                    Category = logComplaintDto.Category,
                    Platform = logComplaintDto.Platform,
                    Description = logComplaintDto.Description
                };

                _unitOfWork.Complaints.AddComplaintAsync(complaint);
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
