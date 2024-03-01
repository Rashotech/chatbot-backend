using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatBot.Models;

namespace ChatBot.Services.Interfaces
{
	public interface IPaymentProvider
	{
        Task<IEnumerable<Bank>> GetBankListAsync();
        Task<AccountResolveData> ResolveAccountAsync(string accountNumber, string bankCode);
    }
}

