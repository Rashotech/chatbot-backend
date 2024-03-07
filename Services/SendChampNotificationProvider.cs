using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ChatBot.Services.Interfaces;

namespace ChatBot.Services
{
    public class SendChampNotificationProvider : INotificationProvider
    {
        private readonly HttpClient _httpClient;

        public SendChampNotificationProvider(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("SendChamp");
        }

        public async Task<string> SendOtpAsync(string phoneNumber)
        {
            using StringContent jsonContent = new(
              JsonSerializer.Serialize(new
              {
                  channel = "sms",
                  sender = "DAlert",
                  token = new Random().Next(100000, 999999).ToString(),
                  token_type = "numeric",
                  token_length = 6,
                  expiration_time = 10,
                  customer_mobile_number = phoneNumber
              }),
              Encoding.UTF8,
              "application/json");

            var response = await _httpClient.PostAsync("api/v1/verification/create", jsonContent);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<SendOtpResponse>(content);

            if (result?.code == 200)
            {
                return result.data.reference;
            }
            else
            {
                throw new HttpRequestException("Unable to Send Otp");
            }
        }

        public async Task<bool> VerifyOtpAsyc(string verificationReference, string verificationCode)
        {
            using StringContent jsonContent = new(
               JsonSerializer.Serialize(new
               {
                   verification_reference = verificationReference,
                   verification_code = verificationCode
               }),
               Encoding.UTF8,
               "application/json");

            var response = await _httpClient.PostAsync("api/v1/verification/confirm", jsonContent);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<VerifyOtpResponse>(content);

            if (result?.code == 200 && result.data.status == "verified")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class GeneralResponse
    {
        public int code { get; set; }
        public string message { get; set; }
        public string status { get; set; }
    }

    public class SendOtpResponse : GeneralResponse
    {
        public SendOtpResponseData data { get; set; }
    }

    public class SendOtpResponseData
    {
        public string business_uid { get; set; }
        public string reference { get; set; }
        public string token { get; set; }
        public string status { get; set; }

    }

    public class VerifyOtpResponse : GeneralResponse
    {
        public VerifyOtpResponseData data { get; set; }
    }

    public class VerifyOtpResponseData
    {
        public string status { get; set; }
        public string reference { get; set; }
        public string token { get; set; }
        public string token_duration { get; set; }
    }
}


