using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ChatBot.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using DotNetEnv;
using Newtonsoft.Json;

namespace ChatBot.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly IBot _bot;
        private readonly string azureChatBotSecret;
        private readonly string azureChatUrl;

        public BotController(
            IBotFrameworkHttpAdapter adapter,
            IBot bot
        )
        {
            _adapter = adapter;
            _bot = bot;
            azureChatBotSecret = Env.GetString("AZURE_BOT_SECRET");
            azureChatUrl = Env.GetString("AzureChatBot:Url");
        }

        [HttpPost, HttpGet]
        public async Task PostAsync()
        {
            await _adapter.ProcessAsync(Request, Response, _bot);
        }

        [HttpPost("chatbot")]
        public async Task<ActionResult> ChatBot()
        {
            var secret = azureChatBotSecret;

            HttpClient client = new HttpClient();

            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Post, azureChatUrl);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secret);

            var userId = $"dl_{Guid.NewGuid()}";

            request.Content = new StringContent(
                JsonConvert.SerializeObject(
                    new { User = new { Id = userId } }),
                    Encoding.UTF8,
                    "application/json");

            var response = await client.SendAsync(request);
            string token = String.Empty;

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                token = JsonConvert.DeserializeObject<DirectLineTokenDto>(body).token;
            }

            var config = new ChatConfigDto()
            {
                Token = token,
                UserId = userId
            };

            return Ok(config);
        }
    }
}
