using System;
using ChatBot.Database;
using ChatBot.Database.Models;
using ChatBot.Repositories.Interfaces;

namespace ChatBot.Repositories
{
    public class AccountRepository : RepositoryBase<Account>, IAccountRepository
    {
        public AccountRepository(BankDbContext context) : base(context)
        {
        }
    }
}


