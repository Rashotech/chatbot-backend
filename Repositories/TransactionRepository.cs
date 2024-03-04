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
    public class TransactionRepository : RepositoryBase<Transaction>, ITransactionRepository
    {
        private readonly BankDbContext _DbContext;

        public TransactionRepository(BankDbContext context) : base(context)
        {
            _DbContext = context;
        }

        public async Task<List<Transaction>> GetTransactions(int accountId, int maxNumberPerItemsPage = 5)
        {
            return await _DbContext.Transactions
                .Where(u => u.AccountId == accountId)
                .OrderByDescending(e => e.CreatedAt)
                .Take(maxNumberPerItemsPage)
                .ToListAsync();
        }
    }
}

