using ChatBot.CognitiveModels;
using ChatBot.Database.Models;
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
        private readonly MessagePrompts _messages;

        public MainDialog(
            BankOperationRecognizer cluRecognizer,
            OpenAccountDialog openAccountDialog,
            FundTransferDialog fundTransferDialog,
            CheckAccountBalanceDialog checkAccountBalanceDialog,
            ManageComplaintDialog manageComplaintDialog,
            TransactionHistoryDialog transactionHistoryDialog,
            QnADialog qnADialog,
            FeedbackDialog feedbackDialog,
            ILogger<MainDialog> logger,
            MessagePrompts messages
        )
            : base(nameof(MainDialog))
        {
            _cluRecognizer = cluRecognizer;
            Logger = logger;
            _messages = messages;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(openAccountDialog);
            AddDialog(fundTransferDialog);
            AddDialog(checkAccountBalanceDialog);
            AddDialog(manageComplaintDialog);
            AddDialog(transactionHistoryDialog);
            AddDialog(qnADialog);
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
                Text = _messages.GetRandomMessage(_messages.Greetings),
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.PostBack, title: "Open Account", value: nameof(BankOperationIntent.OpenAccount)),
                    new CardAction(ActionTypes.PostBack, title: "Balance Enquiry", value: nameof(BankOperationIntent.CheckBalance)),
                    new CardAction(ActionTypes.PostBack, title: "Fund Transfer", value: nameof(BankOperationIntent.FundTransfer)),
                    new CardAction(ActionTypes.PostBack, title: "Transaction History", value: nameof(BankOperationIntent.GetTransactionHistory)),
                    new CardAction(ActionTypes.PostBack, title: "Manage Complaint", value: nameof(BankOperationIntent.ManageComplaint)),
                    new CardAction(ActionTypes.PostBack, title: "FAQs", value: nameof(BankOperationIntent.Faq)),
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

                    case nameof(BankOperationIntent.Faq):
                        return await stepContext.BeginDialogAsync(nameof(QnADialog), new QuestionAnswering(), cancellationToken);

                    default:
                        // Catch all for unhandled intents
                        var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way)";
                        var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                        await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                        break;
                }
            }

            if (_cluRecognizer.IsConfigured && userInput != null)
            {
                var cluResult = await _cluRecognizer.RecognizeAsync<BankOperation>(stepContext.Context, cancellationToken);
                var intent = cluResult.GetTopIntent().intent;

                switch (cluResult.GetTopIntent().intent)
                {
                    case BankOperation.Intent.OpenAccount:
                        return await stepContext.BeginDialogAsync(nameof(OpenAccountDialog), null, cancellationToken);
                        
                    case BankOperation.Intent.LogComplain:
                        return await stepContext.BeginDialogAsync(nameof(ManageComplaintDialog), null, cancellationToken);
                        
                    case BankOperation.Intent.FundTransfer:
                        return await stepContext.BeginDialogAsync(nameof(FundTransferDialog), null, cancellationToken);
                        
                    case BankOperation.Intent.CheckBalance:
                        return await stepContext.BeginDialogAsync(nameof(CheckAccountBalanceDialog), null, cancellationToken);

                    case BankOperation.Intent.GetTransactionHistory:
                        return await stepContext.BeginDialogAsync(nameof(TransactionHistoryDialog), null, cancellationToken);

                    default:
                        return await stepContext.BeginDialogAsync(nameof(QnADialog), new QuestionAnswering() { Skip = true }, cancellationToken);
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

            var messageText = _messages.GetRandomMessage(_messages.GoodbyeMessages);
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
