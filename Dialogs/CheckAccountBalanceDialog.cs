using ChatBot.Database.Models;
using ChatBot.Services;
using ChatBot.Services.Interfaces;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBot.Dialogs
{
    public class CheckAccountBalanceDialog : CancelAndHelpDialog
    {
        private readonly IStatePropertyAccessor<Account> _accountInfoAccessor;
        private readonly IAccountService _accountService;
        public CheckAccountBalanceDialog(
            IAccountService accountService,
            ICustomerService customerService,
            AuthDialog authDialog,
            UserState userState
        )
     : base(nameof(CheckAccountBalanceDialog))
        {
            _accountService = accountService;
            _accountInfoAccessor = userState.CreateProperty<Account>("Account");
            AddDialog(authDialog);
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
        AuthenticateUser,
        DisplayAccountBalance,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AuthenticateUser(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(AuthDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> DisplayAccountBalance(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                var account = _accountInfoAccessor.GetAsync(stepContext.Context, () => null, cancellationToken);
                var balance = await _accountService.GetAccountBalanceAsync(account.Id);

                if (balance != null)
                {
                    string accountBalance = $"{ balance.Currency } { balance.Balance:N2}";
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Your account balance is: {accountBalance}"), cancellationToken);
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
