using System;
using ChatBot.Database.Models;
using System.Threading.Tasks;
using ChatBot.Dtos;

namespace ChatBot.Services.Interfaces
{
	public interface IAccountService
	{
        Task<Account> OpenAccountAsync(OpenAccountDto openAccountDto);
        Task<GetBalanceDto> GetAccountBalanceAsync(int accountId);
        Task<bool> DebitAccountAsync(int accountId, decimal amount);
        Task<Customer> GetAccountByAccountNumber(string accountNumber);
    }
}

