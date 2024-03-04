using ChatBot.CognitiveModels;
using ChatBot.Dtos;
using ChatBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly BankOperationRecognizer _cluRecognizer;
        protected readonly ILogger Logger;
        private readonly string ConfirmDlgId = "ConfirmDlgId";
        private readonly string Confirm2DlgId = "Confirm2DlgId";

        public MainDialog(
            BankOperationRecognizer cluRecognizer,
            OpenAccountDialog openAccountDialog,
            FundTransferDialog fundTransferDialog,
            CheckAccountBalanceDialog checkAccountBalanceDialog,
            ManageComplaintDialog manageComplaintDialog,
            TransactionHistoryDialog transactionHistoryDialog,
            FeedbackDialog feedbackDialog,
            ILogger<MainDialog> logger
        )
            : base(nameof(MainDialog))
        {
            _cluRecognizer = cluRecognizer;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(openAccountDialog);
            AddDialog(fundTransferDialog);
            AddDialog(checkAccountBalanceDialog);
            AddDialog(manageComplaintDialog);
            AddDialog(transactionHistoryDialog);
            AddDialog(feedbackDialog);
            AddDialog(new ConfirmPrompt(ConfirmDlgId));
            AddDialog(new ConfirmPrompt(Confirm2DlgId));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                AskForFeedBackStepAsync,
                ProcessFeedBackStepAsync,
                AskForAnotherTransactionStepAsync,
                FinalStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var card = new HeroCard
            {
                Text = "Hello! Welcome to FirstBank. I’m Fibani, your virtual assistant. How can I help you?",
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.PostBack, title: "Open Account", value: nameof(BankOperationIntent.OpenAccount)),
                    new CardAction(ActionTypes.PostBack, title: "Balance Enquiry", value: nameof(BankOperationIntent.CheckBalance)),
                    new CardAction(ActionTypes.PostBack, title: "Fund Transfer", value: nameof(BankOperationIntent.FundTransfer)),
                    new CardAction(ActionTypes.PostBack, title: "Transaction History", value: nameof(BankOperationIntent.GetTransactionHistory)),
                    new CardAction(ActionTypes.PostBack, title: "Manage Complaint", value: nameof(BankOperationIntent.ManageComplaint)),
                },
            };

            // Create an activity with the HeroCard
            var reply = stepContext.Context.Activity.CreateReply();
            reply.Attachments.Add(card.ToAttachment());

            // Prompt the user with the HeroCard
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = reply }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userInput = stepContext.Context.Activity.Text;

            var message = stepContext.Context.Activity.AsMessageActivity();
            if (IsButtonClickActivity(message))
            {
                switch (userInput)
                {
                    case nameof(BankOperationIntent.OpenAccount):
                        return await stepContext.BeginDialogAsync(nameof(OpenAccountDialog), new OpenAccountDto(), cancellationToken);
                        
                    case nameof(BankOperationIntent.FundTransfer):
                        return await stepContext.BeginDialogAsync(nameof(FundTransferDialog), null, cancellationToken);
                        
                    case nameof(BankOperationIntent.CheckBalance):
                        return await stepContext.BeginDialogAsync(nameof(CheckAccountBalanceDialog), null, cancellationToken);
                        
                    case nameof(BankOperationIntent.ManageComplaint):
                        return await stepContext.BeginDialogAsync(nameof(ManageComplaintDialog), null, cancellationToken);

                    case nameof(BankOperationIntent.GetTransactionHistory):
                        return await stepContext.BeginDialogAsync(nameof(TransactionHistoryDialog), null, cancellationToken);

                    default:
                        // Catch all for unhandled intents
                        var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way)";
                        var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                        await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                        break;
                }
            }

            if (!_cluRecognizer.IsConfigured && userInput != null)
            {
                var cluResult = await _cluRecognizer.RecognizeAsync<BankOperation>(stepContext.Context, cancellationToken);
                var intent = cluResult.GetTopIntent().intent;

                switch (cluResult.GetTopIntent().intent)
                {
                    case BankOperation.Intent.AccountOpening:
                        return await stepContext.BeginDialogAsync(nameof(OpenAccountDialog), null, cancellationToken);
                        
                    case BankOperation.Intent.ManageComplaint:
                        return await stepContext.BeginDialogAsync(nameof(ManageComplaintDialog), null, cancellationToken);
                        
                    case BankOperation.Intent.FundTransfer:
                        return await stepContext.BeginDialogAsync(nameof(FundTransferDialog), null, cancellationToken);
                        
                    case BankOperation.Intent.CheckingBalance:
                        return await stepContext.BeginDialogAsync(nameof(CheckAccountBalanceDialog), null, cancellationToken);

                    case BankOperation.Intent.GetTransactionHistory:
                        return await stepContext.BeginDialogAsync(nameof(TransactionHistoryDialog), null, cancellationToken);

                    default:
                        // Catch all for unhandled intents
                        // TODO: Integrate question answering here
                        var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way (intent was {cluResult.GetTopIntent().intent})";
                        var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                        await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                        break;
                }
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> AskForFeedBackStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var messageText = "Would you like to share feedback?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(ConfirmDlgId, new PromptOptions
            {
                Prompt = promptMessage
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> ProcessFeedBackStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result != false)
            {
                return await stepContext.BeginDialogAsync(nameof(FeedbackDialog), new OpenAccountDto(), cancellationToken);
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> AskForAnotherTransactionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var messageText = "Do you want to carry out another transaction?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(Confirm2DlgId, new PromptOptions
            {
                Prompt = promptMessage
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result != false)
            {
                return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
            }

            var messageText = $"Thanks for banking with us";
            var endMessage = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
            await stepContext.Context.SendActivityAsync(endMessage, cancellationToken);
            return await stepContext.CancelAllDialogsAsync(cancellationToken);
        }

        public static bool IsButtonClickActivity(IMessageActivity activity)
            {
                if (activity.Type == ActivityTypes.Message && activity.ChannelData != null)
                {
                    JObject channelData = JObject.FromObject(activity.ChannelData);
                    if (channelData["postBack"] != null)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
}
