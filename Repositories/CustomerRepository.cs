using System;
using ChatBot.Database;
using ChatBot.Database.Models;
using ChatBot.Repositories.Interfaces;

namespace ChatBot.Repositories
{
	public class CustomerRepository : RepositoryBase<Customer>, ICustomerRepository
    {
        public CustomerRepository(BankDbContext context): base(context)
        {
        }
    }
}

