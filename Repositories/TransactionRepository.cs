using System;
using ChatBot.Database;
using ChatBot.Database.Models;
using ChatBot.Repositories.Interfaces;

namespace ChatBot.Repositories
{
    public class TransactionRepository : RepositoryBase<Transaction>, ITransactionRepository
    {
        public TransactionRepository(BankDbContext context) : base(context)
        {
        }
    }
}

