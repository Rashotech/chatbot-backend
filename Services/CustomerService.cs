using System;
using System.Threading.Tasks;
using ChatBot.Database.Models;
using ChatBot.Repositories.Interfaces;
using ChatBot.Services.Interfaces;

namespace ChatBot.Services
{
	public class CustomerService: ICustomerService
	{
        private readonly IUnitOfWork _unitOfWork;

        public CustomerService(IUnitOfWork unitOfWork)
		{
            _unitOfWork = unitOfWork;
        }

        public async Task CreateCustomer(Customer customer)
        {
            _unitOfWork.Customers.Add(customer);
            await _unitOfWork.CommitAsync();
        }

        public async Task<Customer> GetCustomerInfoAsync(int customerId)
        {
            return await _unitOfWork.Customers.GetCustomerInfo(customerId);
        }
    }
}

