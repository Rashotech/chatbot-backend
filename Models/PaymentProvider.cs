using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ChatBot.Models
{
    public class GeneralResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class Bank
	{
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("slug")]
        public string Slug { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }
    }

    public class BankListResponse: GeneralResponse
    {
        [JsonPropertyName("data")]
        public List<Bank> Data { get; set; }
    }

    public class AccountResolveResponse: GeneralResponse
    {
        public AccountResolveData Data { get; set; }
    }

    public class AccountResolveData
    {
        [JsonPropertyName("account_number")]
        public string AccountNumber { get; set; }

        [JsonPropertyName("account_name")]
        public string AccountName { get; set; }
    }
}

