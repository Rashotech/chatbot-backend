using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatBot.Database.Models;

namespace ChatBot.Repositories.Interfaces
{
	public interface IAccountRepository : IBaseRepository<Account>
    {
        Task<List<Account>> GetAllAccountsByUserId(int customerId);
        Task<Account> GetSingleAccount(int accountId);
        Task<Account> GetAccountByAccountNumber(string accountNumber);
    }
}

