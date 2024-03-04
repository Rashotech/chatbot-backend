using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatBot.Database.Models;
using ChatBot.Dtos;
using ChatBot.Repositories.Interfaces;
using ChatBot.Services.Interfaces;

namespace ChatBot.Services
{
	public class TransactionService: ITransactionService
	{
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAccountService _accountService;

        public TransactionService(IUnitOfWork unitOfWork, IAccountService accountService)
        {
            _unitOfWork = unitOfWork;
            _accountService = accountService;
        }

        public async Task<IEnumerable<TransactionDto>> GetAccountTransactionsAsync(int accountId, int limit = 5)
        {
            var transactions = await _unitOfWork.Transactions.GetTransactions(accountId, limit);
            return transactions.Select(t => new TransactionDto
            {
                AccountId = t.AccountId,
                TransactionReference = t.TransactionReference,
                TransactionType = t.TransactionType.ToString(),
                Direction = t.Direction.ToString(),
                Channel = t.Channel.ToString(),
                Status = t.Status.ToString(),
                Amount = (int)Math.Round(t.Amount / 100m, 2),
                Narration = t.Narration,
                Date = t.CreatedAt,
                Currency = "NGN",
                RecipientAccountNumber = t.RecipientAccountNumber,
                RecipientBankCode = t.RecipientBankCode,
                RecipientName = t.RecipientName,
                RecipientBankName = t.RecipientBankName
            });
        }


        public async Task<Transaction> FundTransfer(FundTransferDto fundTransferDto)
        {
            try
            {
                await _accountService.DebitAccountAsync(fundTransferDto.AccountId, fundTransferDto.Amount);

                var transaction = new Transaction
                {
                    TransactionReference = GenerateTransactionRef(),
                    AccountId = fundTransferDto.AccountId,
                    Amount = (int) fundTransferDto.Amount * 100,
                    TransactionType = TransactionType.Transfer,
                    Direction = TransactionDirection.Debit,
                    Channel = fundTransferDto.Channel,
                    Status = TransactionStatus.Successful,
                    Narration = fundTransferDto.Narration,
                    RecipientAccountNumber = fundTransferDto.RecipientAccountNumber,
                    RecipientBankCode = fundTransferDto.RecipientBankCode,
                    RecipientName = fundTransferDto.RecipientName,
                    RecipientBankName = fundTransferDto.RecipientBankName
                };
                _unitOfWork.Transactions.Add(transaction);
                await _unitOfWork.CommitAsync();
                return transaction;
            }
            catch(Exception)
            {
                throw;

            }
        }

        public async Task<Transaction> GetTransactionByReferenceAsync(int accountId, string transactionReference)
		{
			try
			{
                return await _unitOfWork.Transactions.GetTransactionByReferenceAsync(accountId, transactionReference);
            }
			catch (Exception)
			{
				return null;
			}
		}

        private static string GenerateTransactionRef()
        {
            string prefix = "TRX";
            string timestampPart = DateTime.Now.ToString("yyyyMMddHHmm");
            string randomPart = new Random().Next(1000, 9999).ToString();

            return $"{prefix}{timestampPart}{randomPart}";
        }
    }
}

