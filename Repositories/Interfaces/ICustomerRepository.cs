using System;
using System.Threading.Tasks;
using ChatBot.Database.Models;

namespace ChatBot.Repositories.Interfaces
{
	public interface ICustomerRepository: IBaseRepository<Customer>
    {
        Task<Customer> GetCustomerInfo(int customerId);
    }
}

