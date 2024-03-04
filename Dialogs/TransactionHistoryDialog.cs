using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;
using ChatBot.Services.Interfaces;
using ChatBot.Database.Models;
using ChatBot.Utils;
using System.Linq;

namespace ChatBot.Dialogs
{
    public class TransactionHistoryDialog : CancelAndHelpDialog
    {
        private readonly IStatePropertyAccessor<Account> _accountInfoAccessor;
        private readonly ITransactionService _transactionService;
        private readonly string TransactionAmountDlgId = "TransactionAmountDlgId";
        private readonly string PinDlgId = "PinDlgId";
        private readonly string RequestRecipientAccountNumberDlgId = "RequestRecipientAccountNumberDlgId";
        private readonly string NarrationDlgId = "NarrationDlgId";

        public TransactionHistoryDialog(
            ITransactionService transactionService,
            UserState userState,
            AuthDialog authDialog
        )
        : base(nameof(TransactionHistoryDialog))
        {
            _accountInfoAccessor = userState.CreateProperty<Account>("Account");
            _transactionService = transactionService;
            AddDialog(authDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AuthenticationStepAsync,
                InitiateTransactionHistoryStepAsync,
                TransactionHistoryStepAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AuthenticationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(AuthDialog), null, cancellationToken);
        }

        private static async Task<DialogTurnResult> InitiateTransactionHistoryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var text = "Kindly wait while we fetch your last 5 transactions";

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(text), cancellationToken);

            return await stepContext.NextAsync(null, cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> TransactionHistoryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var account = await _accountInfoAccessor.GetAsync(stepContext.Context, () => null, cancellationToken);
            var transactions = await _transactionService.GetAccountTransactionsAsync(account.Id);

            if(transactions.Any())
            {
                var adaptiveCardAttachment = AdaptiveCardHelper.CreateTableAdaptiveCardAttachment("TransactionTableCard", transactions,
                     getNarration: trx => trx.Narration,
                     getAmount: trx => $"{trx.Direction} {trx.Currency} {trx.Amount}",
                     getDate: trx => trx.Date.ToString("dddd, MMMM d, yyyy hh:mmtt")
               );

                var message = MessageFactory.Attachment(adaptiveCardAttachment);
                await stepContext.Context.SendActivityAsync(message, cancellationToken);
            }
            else
            {
                var text = "No Transactions yet, Kindly carry out a transaction today!";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(text), cancellationToken);
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}






