using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatBot.Database.Models;
using ChatBot.Dtos;
using ChatBot.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChatBot.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IAccountService _accountService;
        private readonly ITransactionService _transactionService;
        private readonly IPaymentProvider _paymentProvider;
        private readonly INotificationProvider _notificationProvider;

        public TestController(
            ICustomerService customerService,
            IAccountService accountService,
            ITransactionService transactionService,
            IPaymentProvider paymentProvider,
            INotificationProvider notificationProvider
        )
        {
            _customerService = customerService;
            _accountService = accountService;
            _transactionService = transactionService;
            _paymentProvider = paymentProvider;
            _notificationProvider = notificationProvider;
        }

        [HttpGet("account")]
        public async Task<IActionResult> CreateAccount()
        {
            var openAccountDto = new OpenAccountDto
            {
                FirstName = "Samuel",
                LastName = "Okafor",
                OtherName = "Nwachukwu",
                Email = "samuelokafor@gmail.com",
                PhoneNumber = "0811844846",
                Address = "7 Iletunmi St., Ikeja, Lagos",
                BVNNumber = "22425041221",
                NIN = "12345678935",
                DateOfBirth = new DateTime(1996, 03, 02),
                AccountType = AccountType.Savings
            };
            
            var account = await _accountService.OpenAccountAsync(openAccountDto);
            return Ok(account);
        }

        [HttpGet("transfer")]
        public async Task<IActionResult> FundTransfer()
        {
            var fundTransferDto = new FundTransferDto
            {
                AccountId = 1,
                Amount = 200,
                Narration = "School Fee",
                RecipientAccountNumber = "0237013933",
                RecipientBankCode = "044",
                RecipientBankName = "Guaranty Trust Bank",
                RecipientName = "Ayoade Rasheed Adedamola"
            };

            var transaction = await _transactionService.FundTransfer(fundTransferDto);
            return Ok(transaction);
        }

        [HttpGet("accounts")]
        public async Task<IActionResult> GetAccount()
        {
            var customer = await _customerService.GetCustomerInfoAsync(3);
            Console.WriteLine(customer.Accounts);
            var response = new CustomerDto
            {
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                // Map other properties as needed
                Accounts = customer.Accounts.Select(account => new AccountDto
                {
                    AccountNumber = account.AccountNumber,
                    AccountType = account.AccountType.ToString(),
                    AccountStatus = account.AccountStatus.ToString()
                }).ToList()
            };
            return Ok(response);
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetAccountTransactionsAsync()
        {
            var transactions = await _transactionService.GetAccountTransactionsAsync(1);
            return Ok(transactions);
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetAccountBalanceAsync()
        {
            var balance = await _accountService.GetAccountBalanceAsync(3);
            return Ok(balance);
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> GetAccountByAccountNumber()
        {
            var account = await _accountService.GetAccountByAccountNumber("3121539729");
            return Ok(account);
        }


        [HttpGet("banks")]
        public async Task<IActionResult> GetBankList()
        {
            var banks = await _paymentProvider.GetBankListAsync();
            return Ok(banks);
        }

        [HttpGet("banks/resolve")]
        public async Task<IActionResult> ResolveAccountAsync()
        {
            var account = await _paymentProvider.ResolveAccountAsync("3073719356", "011");
            return Ok(account);
        }

        [HttpGet("otp/send")]
        public async Task<IActionResult> SendOtpAsync()
        {
            var result = await _notificationProvider.SendOtpAsync("2348133166978");
            return Ok(result);
        }

        [HttpGet("otp/confirm")]
        public async Task<IActionResult> VerifyOtpAsync()
        {
            var result = await _notificationProvider.VerifyOtpAsyc("MN-OTP-9bc25b94-7417-43c8-9b1b-d7c49f532187", "243510");
            return Ok(result);
        }
    }

    public class CustomerDto
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string OtherName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string BVNNumber { get; set; }

        public string NIN { get; set; }

        public string Address { get; set; }

        public DateTime DateOfBirth { get; set; }

        public List<AccountDto> Accounts { get; set; }
    }

    public class AccountDto
    {
        public string AccountNumber { get; set; }

        public string AccountType { get; set; }

        public string AccountStatus { get; set; }

        public int Balance { get; set; }
    }
}
