using System;
using System.Linq;

namespace ChatBot.Utils
{
	public class AccountNumberGenerator
	{
        private static readonly Random _random = new Random();

        public static string GenerateRandomAccountNumber()
        {
            const string prefix = "3121";
            const string bankCode = "011";
            int[] dictionary = { 3, 7, 3, 3, 7, 3, 3, 7, 3, 3, 7, 3 };

            string accountSerialNo = prefix + GenerateRandomNDigits(4);
            string nubanAccountFormat = bankCode + accountSerialNo;

            int checkSum = 0;
            foreach (var (charDigit, index) in nubanAccountFormat.Select((c, i) => (c, i)))
            {
                checkSum += (charDigit - '0') * dictionary[index]; // Convert characters to digits
            }

            int validatedCheckDigit = 10 - (checkSum % 10);

            return accountSerialNo + validatedCheckDigit;
        }

        private static string GenerateRandomNDigits(int n)
        {
            int lowerBound = (int)Math.Pow(10, n - 1);  // Adjusted calculation 
            int upperBound = (int)Math.Pow(10, n) * 9;   // Equivalent to '9 * (Math.pow(10, n))'

            return _random.Next(lowerBound, upperBound).ToString();
        }
    }
}

