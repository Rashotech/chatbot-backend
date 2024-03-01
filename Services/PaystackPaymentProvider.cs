using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ChatBot.Models;
using ChatBot.Services.Interfaces;

namespace ChatBot.Services
{
	public class PaystackPaymentProvider : IPaymentProvider
    {
        private readonly HttpClient _httpClient;

        public PaystackPaymentProvider(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Paystack");
        }

        public async Task<IEnumerable<Bank>> GetBankListAsync()
        {
            var response = await _httpClient.GetAsync("bank");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<BankListResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (result?.Status == true)
            {
                return result.Data;
            }
            else
            {
                throw new Exception("Failed to fetch banks: " + result?.Message);
            }
        }

        public async Task<AccountResolveData> ResolveAccountAsync(string accountNumber, string bankCode)
        {
            var response = await _httpClient.GetAsync($"bank/resolve?account_number={accountNumber}&bank_code={bankCode}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AccountResolveResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            Console.WriteLine(result);

            if (result?.Status == true)
            {
                return result.Data;
            }
            else
            {
                throw new Exception("Failed to resolve account: " + result?.Message);
            }
        }
    }
}

