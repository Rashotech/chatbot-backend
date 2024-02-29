using System;
namespace ChatBot.Exceptions
{
    public class AccountNotFound : Exception
    {
        public AccountNotFound() : base("Account Not Found")
        {
        }

        public AccountNotFound(string message) : base(message)
        {
        }

        public AccountNotFound(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

