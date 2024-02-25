using System;
using ChatBot.Database.Models;
using ChatBot.Repositories.Interfaces;
using System.Threading.Tasks;
using ChatBot.Services.Interfaces;
using ChatBot.Dtos;
using ChatBot.Utils;

namespace ChatBot.Services
{
	public class AccountService: IAccountService
	{
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICustomerService _customerService;

        public AccountService(IUnitOfWork unitOfWork, ICustomerService customerService)
		{
            _unitOfWork = unitOfWork;
            _customerService = customerService;
        }

        public async Task<Account> OpenAccountAsync(OpenAccountDto openAccountDto)
        {
            var customer = new Customer
            {
                FirstName = openAccountDto.FirstName,
                LastName = openAccountDto.LastName,
                OtherName = openAccountDto.OtherName,
                Email = openAccountDto.Email,
                PhoneNumber = openAccountDto.PhoneNumber,
                Address = openAccountDto.Address,
                BVNNumber = openAccountDto.BVNNumber,
                NIN = openAccountDto.NIN,
                DateOfBirth = openAccountDto.DateOfBirth,
            };
            await _customerService.CreateCustomer(customer);

            var accountNumber = AccountNumberGenerator.GenerateRandomAccountNumber();
            var account = new Account
            {
                AccountNumber = accountNumber,
                AccountType = openAccountDto.AccountType,
                Customer = customer
            };

            _unitOfWork.Accounts.Add(account);
            await _unitOfWork.CommitAsync();
            return account;
        }

        public async Task<GetBalanceDto> GetAccountBalanceAsync(int accountId)
        {
            try
            {
                var account = await _unitOfWork.Accounts.GetSingleAccount(accountId);
                if (account == null) throw new ArgumentException("Account not found", nameof(accountId));

                return new GetBalanceDto
                {
                    Balance = Math.Round(account.Balance / 100m, 2),
                    Currency = account.Currency
                };
            }
            catch (Exception)
            {
                throw;
            }
           
        } 
    }
}

