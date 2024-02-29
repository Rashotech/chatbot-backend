using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;
using ChatBot.Dtos;
using System.Collections.Generic;
using Newtonsoft.Json;
using ChatBot.Services.Interfaces;

namespace ChatBot.Dialogs
{
    public class GetTransactionHistoryDialog : CancelAndHelpDialog
    {
        private const string TextPromptId = "text";
        private readonly ITransactionService _transactionService;

        public GetTransactionHistoryDialog(ITransactionService transactionService)
            : base(nameof(GetTransactionHistoryDialog))
        {
            _transactionService = transactionService;
            AddDialog(new TextPrompt(TextPromptId));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PromptForAccountNumberStepAsync,
                ConfirmStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> PromptForAccountNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Please enter your account number."),
            };

            return await stepContext.PromptAsync(TextPromptId, promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var accountNumber = stepContext.Result.ToString();

            // You can perform additional validation on the accountNumber if needed.

            stepContext.Values["AccountNumber"] = accountNumber;

            var promptMessage = $"You entered account number: {accountNumber}. Is this correct?";
            var confirmPromptOptions = new PromptOptions { Prompt = MessageFactory.Text(promptMessage), RetryPrompt = MessageFactory.Text("Please confirm with 'yes' or 'no'.") };

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), confirmPromptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                if ((bool)stepContext.Result)
                {
                    var accountNumber = (string)stepContext.Values["AccountNumber"];

                    Console.WriteLine(accountNumber);

                    // Call your transaction service to get the transaction history based on the accountNumber
                    var transactions = await _transactionService.GetTransactionHistoryAsync(accountNumber);

                    // Process the retrieved transactions as needed

                    var response = $"Transaction history for account {accountNumber} retrieved successfully. Displaying details: {JsonConvert.SerializeObject(transactions)}";
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(response), cancellationToken);

                    return await stepContext.EndDialogAsync(null, cancellationToken);
                }

                var promptMessage = "Kindly provide the correct account number.";
                return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
            }
            catch (Exception)
            {
                var promptMessage = "Unable to retrieve transaction history. Something went wrong. Please try again.";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(promptMessage), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }
    }
}
