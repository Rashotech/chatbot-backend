using System;
using ChatBot.Database.Models;
using ChatBot.Repositories.Interfaces;
using System.Threading.Tasks;
using ChatBot.Services.Interfaces;
using ChatBot.Dtos;
using ChatBot.Utils;
using ChatBot.Exceptions;

namespace ChatBot.Services
{
	public class AccountService: IAccountService
	{
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICustomerService _customerService;
        private readonly string defaultOtp = "123456";
        private readonly string defaultPin = "1234";

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

        public async Task<bool> DebitAccountAsync(int accountId, decimal amount)
        {
            try
            {
                // Retrieve the account from the database
                var account = await _unitOfWork.Accounts.GetSingleAccount(accountId);

                // Ensure that the account exists
                if (account == null)
                    throw new AccountNotFound("Account not found");

                // Verify if the account has sufficient balance
                if (account.Balance < amount * 100) // Assuming the balance is stored in cents
                    throw new InvalidOperationException("Insufficient balance");

                // Perform the debit operation
                account.Balance -= (int)(amount * 100); // Deducting the amount from the balance

                // Update the account balance in the database
                _unitOfWork.Accounts.Update(account);
                await _unitOfWork.CommitAsync();

                return true; // Debit operation successful
            }
            catch (Exception)
            {
                throw; // Propagate the exception
            }
        }

        public async Task<Account> GetAccountByAccountNumber(string accountNumber)
        {
            var account = await _unitOfWork.Accounts.GetAccountByAccountNumber(accountNumber);
            if (account == null) throw new AccountNotFound("Account not found");
            return account;
        }

        public bool ValidateOtp(string otp)
        {
            if (string.IsNullOrEmpty(otp) || otp.Length != 6 || otp != defaultOtp)
            {
                return false;
            }

            return true;
        }

        public bool ValidatePin(string pin)
        {
            if (string.IsNullOrEmpty(pin) || pin.Length != 4 || pin != defaultPin)
            {
                return false;
            }

            return true;
        }


        public async Task<GetBalanceDto> GetAccountBalanceAsync(int accountId)
        {
            try
            {
                var account = await _unitOfWork.Accounts.GetSingleAccount(accountId);
                if (account == null) throw new AccountNotFound("Account not found");

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

