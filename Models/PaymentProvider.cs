using System.Collections.Generic;

namespace ChatBot.Models
{
    public class GeneralResponse
    {
        public bool status { get; set; }
        public string message { get; set; }
    }

    public class Bank
	{
        public string name { get; set; }
        public string code { get; set; }
    }

    public class BankListResponse: GeneralResponse
    {
        public List<Bank> data { get; set; }
    }

    public class AccountResolveResponse: GeneralResponse
    {
        public AccountResolveData data { get; set; }
    }

    public class AccountResolveData
    {
        public string account_number { get; set; }
        public string account_name { get; set; }
    }
}

