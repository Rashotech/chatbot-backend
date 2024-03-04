using ChatBot.CognitiveModels;
using ChatBot.Dtos;
using ChatBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly BankOperationRecognizer _cluRecognizer;
        protected readonly ILogger Logger;

        public MainDialog(
            BankOperationRecognizer cluRecognizer,
            OpenAccounDialog openAccounDialog,
            CheckAccountBalanceDialog checkAccountBalanceDialog,
            ManageComplaintDialog manageComplaintDialog,
            ILogger<MainDialog> logger
        )
            : base(nameof(MainDialog))
        {
            _cluRecognizer = cluRecognizer;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(openAccounDialog);
            AddDialog(checkAccountBalanceDialog);
            AddDialog(manageComplaintDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
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
                        return await stepContext.BeginDialogAsync(nameof(OpenAccounDialog), new OpenAccountDto(), cancellationToken);
                    case nameof(BankOperationIntent.CheckBalance):
                        return await stepContext.BeginDialogAsync(nameof(CheckAccountBalanceDialog), null, cancellationToken);
                        
                    case nameof(BankOperationIntent.ManageComplaint):
                        return await stepContext.BeginDialogAsync(nameof(ManageComplaintDialog), null, cancellationToken);

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
                        return await stepContext.BeginDialogAsync(nameof(OpenAccounDialog), null, cancellationToken);

                    case BankOperation.Intent.ManageComplaint:
                        return await stepContext.BeginDialogAsync(nameof(ManageComplaintDialog), null, cancellationToken);
                    case BankOperation.Intent.CheckingBalance:
                        return await stepContext.BeginDialogAsync(nameof(CheckAccountBalanceDialog), null, cancellationToken);

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

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var messageText = $"Thanks for banking with us";
            var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
            await stepContext.Context.SendActivityAsync(message, cancellationToken);

            var promptMessage = "What else can I do for you?";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
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
