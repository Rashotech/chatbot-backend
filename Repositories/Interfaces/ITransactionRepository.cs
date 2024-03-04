using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatBot.Database.Models;

namespace ChatBot.Repositories.Interfaces
{
	public interface ITransactionRepository : IBaseRepository<Transaction>
    {
        Task<List<Transaction>> GetTransactions(int accountId, int maxNumberPerItemsPage = 5);
        Task<Transaction> GetTransactionByReferenceAsync(int accountId, string transactionReference);
    }
}

