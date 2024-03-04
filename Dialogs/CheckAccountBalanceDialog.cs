using ChatBot.Database.Models;
using ChatBot.Services.Interfaces;
using ChatBot.Utils;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBot.Dialogs
{
    public class CheckAccountBalanceDialog : CancelAndHelpDialog
    {
        private const string AdaptivePromptId = "adaptive";
        private readonly IStatePropertyAccessor<Account> _accountInfoAccessor;
        private readonly IAccountService _accountService;
        private readonly ICustomerService _customerService;
        public CheckAccountBalanceDialog(
            IAccountService accountService,
            ICustomerService customerService,
            AuthDialog authDialog,
            UserState userState
        )
     : base(nameof(CheckAccountBalanceDialog))
        {
            _accountService = accountService;
            _customerService = customerService;
            _accountInfoAccessor = userState.CreateProperty<Account>("Account");
            AddDialog(new AdaptiveCardPrompt(AdaptivePromptId));
            AddDialog(authDialog);
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AuthenticateUser,
                AccountBalanceNoticeAsync,
                DisplayAccountBalance,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AuthenticateUser(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(AuthDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> AccountBalanceNoticeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please wait while we fetch your account balance..."), cancellationToken);
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> DisplayAccountBalance(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                var account = await _accountInfoAccessor.GetAsync(stepContext.Context, () => null, cancellationToken);
                var balance = await _accountService.GetAccountBalanceAsync(account.Id);
                var customer = await _customerService.GetCustomerInfoAsync(account.CustomerId);

                if (balance != null)
                {
                    string accountName = $"{customer.FirstName} {customer.OtherName} {customer.LastName}";
                    string accountBalance = $"{balance.Currency} {balance.Balance:N2}";


                    var adaptiveCardJson = @"
        {
            ""type"": ""AdaptiveCard"",
            ""body"": [
                {
                    ""type"": ""TextBlock"",
                    ""text"": ""Account Balance"",
                    ""weight"": ""Bolder"",
                    ""size"": ""Medium""
                    
                },
                {
                    ""type"": ""TextBlock"",
                    ""text"": ""Account Name: ${accountName}"",
                    ""size"": ""Medium"",
                    ""weight"": ""Bolder""
                },
                {
                    ""type"": ""TextBlock"",
                    ""text"": ""Balance: ${balance}"",
                    ""size"": ""ExtraLarge"",
                    ""weight"": ""Bolder"",
                    ""spacing"": ""Small""
                }
            ]
        }";
                    adaptiveCardJson = adaptiveCardJson.Replace("${accountName}", accountName);
                    adaptiveCardJson = adaptiveCardJson.Replace("${balance}", accountBalance);
                    adaptiveCardJson = adaptiveCardJson.Replace("${accountType}", accountBalance);

                    var adaptiveCardAttachment = new Attachment()
                    {
                        ContentType = "application/vnd.microsoft.card.adaptive",
                        Content = JObject.Parse(adaptiveCardJson)
                    };

                    var message = MessageFactory.Attachment(adaptiveCardAttachment);

                    await stepContext.Context.SendActivityAsync(message, cancellationToken);

                }
                else
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Failed to retrieve account balance."), cancellationToken);
                }

                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            catch (Exception ex)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(ex.Message), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }
    }
}


