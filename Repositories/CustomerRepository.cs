using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatBot.Database;
using ChatBot.Database.Models;
using ChatBot.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatBot.Repositories
{
	public class CustomerRepository : RepositoryBase<Customer>, ICustomerRepository
    {
        private readonly BankDbContext _DbContext;

        public CustomerRepository(BankDbContext context): base(context)
        {
            _DbContext = context;
        }

        public async Task<Customer> GetCustomerInfo(int customerId)
        {
            return await _DbContext.Customers
                       .Include(a => a.Accounts)
                       .SingleOrDefaultAsync(a => a.Id == customerId);
        }
    }
}

