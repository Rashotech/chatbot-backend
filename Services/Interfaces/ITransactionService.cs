using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatBot.Database.Models;
using ChatBot.Dtos;

namespace ChatBot.Services.Interfaces
{
	public interface ITransactionService
	{
		Task<Transaction> FundTransfer(FundTransferDto fundTransferDto);
		Task<IEnumerable<TransactionDto>> GetAccountTransactionsAsync(int accountId, int page = 1, int limit = 5);

		Task<List<Transaction>> GetTransactionsByReferenceAsync(int accountId, string transactionReference);

	}
}

