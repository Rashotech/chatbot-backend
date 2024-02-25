using System;
using ChatBot.Database.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ChatBot.Services.Interfaces
{
	public interface ICustomerService
	{
        Task CreateCustomer(Customer customer);
        Task<Customer> GetCustomerInfoAsync(int customerId);
    }
}

