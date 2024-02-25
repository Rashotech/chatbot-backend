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
    public class AccountRepository : RepositoryBase<Account>, IAccountRepository
    {
        private readonly BankDbContext _DbContext;

        public AccountRepository(BankDbContext context) : base(context)
        {
            _DbContext = context;
        }

        public async Task<Account> GetSingleAccount(int accountId)
        {
            return await _DbContext.Accounts.SingleOrDefaultAsync(a => a.Id == accountId);
        }

        public async Task<List<Account>> GetAllAccountsByUserId(int customerId)
        {
            return await _DbContext.Accounts.Where(u => u.CustomerId == customerId).ToListAsync();
        }
    }
}


