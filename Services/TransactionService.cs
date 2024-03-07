using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ChatBot.Database.Models;
using ChatBot.Dtos;
using ChatBot.Repositories.Interfaces;
using ChatBot.Services.Interfaces;
using Newtonsoft.Json;
using DotNetEnv;

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

                if(fundTransferDto.AccountId == 2 && fundTransferDto.Amount < 300)
                {
                    await TransferFundToBanksAsync(fundTransferDto);
                }

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

        public async Task<bool> TransferFundToBanksAsync(FundTransferDto fundTransferDto)
        {
            var baseUrl = Env.GetString("FLW_BASE_URL");
            var secret = Env.GetString("FLW_SECRET_KEY");

            HttpClient client = new HttpClient();

            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Post, baseUrl);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secret);

            var reference = Guid.NewGuid();

            request.Content = new StringContent(
                JsonConvert.SerializeObject(
                    new
                    {
                        account_bank = fundTransferDto.RecipientBankCode,
                        account_number = fundTransferDto.RecipientAccountNumber,
                        amount = fundTransferDto.Amount,
                        narration = fundTransferDto.Narration,
                        currency = "NGN",
                        debit_currency = "NGN",
                        reference
                    }),
                    Encoding.UTF8,
                    "application/json");

            var response = await client.SendAsync(request);
            string status = String.Empty;

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                status = JsonConvert.DeserializeObject<ResponseData>(body).status;
            }

            return status == "success";
        }

        private static string GenerateTransactionRef()
        {
            string prefix = "TRX";
            string timestampPart = DateTime.Now.ToString("yyyyMMddHHmm");
            string randomPart = new Random().Next(1000, 9999).ToString();

            return $"{prefix}{timestampPart}{randomPart}";
        }
    }

    public class ResponseData
    {
        public string status { get; set; }
        public string message { get; set; }
    }
}

