using System;
using System.Threading.Tasks;

namespace ChatBot.Services.Interfaces
{
	public interface INotificationProvider
	{
        Task<string> SendOtpAsync(string phoneNumber);
        Task<bool> VerifyOtpAsyc(string verificationReference, string verificationCode);
    }
}

