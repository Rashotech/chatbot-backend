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
		Task<Transaction> GetTransactionByReferenceAsync(int accountId, string transactionReference);
		Task<IEnumerable<TransactionDto>> GetAccountTransactionsAsync(int accountId, int limit = 5);
	}
}

