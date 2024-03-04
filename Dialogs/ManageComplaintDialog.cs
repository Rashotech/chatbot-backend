using ChatBot.Database.Models;
using ChatBot.Models;
using ChatBot.Repositories.Interfaces;
using ChatBot.Services.Interfaces;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace ChatBot.Dialogs
{
    public class ManageComplaintDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<Account> _accountInfoAccessor;
        private readonly ICustomerService _customerService;

        public ManageComplaintDialog(
            BankOperationRecognizer cluRecognizer,
            LogComplaintDialog logComplaintDialog,
            TrackComplaintDialog trackComplaintDialog,
            ICustomerService customerService,
            IComplaintRepository complaintRepository,
            AuthDialog authDialog,
            UserState userState
        )
     : base(nameof(ManageComplaintDialog))
        {
            _customerService = customerService;
            _accountInfoAccessor = userState.CreateProperty<Account>("Account");
            AddDialog(authDialog);
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(trackComplaintDialog);
            AddDialog(logComplaintDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AuthenticateUserAsync,
                ConfirmAuthenticationAsync,
                SelectOptionAsync,
                RedirectAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AuthenticateUserAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("Let's take some seconds to validate the account for which you would like to manage your complaint.");
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            return await stepContext.BeginDialogAsync(nameof(AuthDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmAuthenticationAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var account = await _accountInfoAccessor.GetAsync(stepContext.Context, () => null, cancellationToken);
            stepContext.Values["Account"] = account;
            Customer customer = await _customerService.GetCustomerInfoAsync(account.CustomerId);

            var reply = MessageFactory.Text($"Yeap! {customer.FirstName} I found your account\n\n" +
                $"Lets move on");
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            return await stepContext.NextAsync(reply, cancellationToken);
        }

        private async Task<DialogTurnResult> SelectOptionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var card = new HeroCard
            {
                Text = "So, would you like to Make a complaint or Track the status of one youve made before?",
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.PostBack, title: "Make A New Complaint", value: "LogComplaint"),
                    new CardAction(ActionTypes.PostBack, title: "Track Complaint Status", value: "TrackComplaintStatus"),
                },
            };


            var reply = stepContext.Context.Activity.CreateReply();
            reply.Attachments.Add(card.ToAttachment());

            // Prompt the user with the HeroCard
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = reply }, cancellationToken);
        }

        private async Task<DialogTurnResult> RedirectAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userInput = stepContext.Context.Activity.Text;

            switch (userInput)
            {
                case "LogComplaint":
                    return await stepContext.BeginDialogAsync(nameof(LogComplaintDialog), null, cancellationToken);

                case "TrackComplaintStatus":
                    return await stepContext.BeginDialogAsync(nameof(TrackComplaintDialog), null, cancellationToken);

                default:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way)";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                    break;
            }
     
            return await stepContext.NextAsync(null, cancellationToken);
        }
    }
}
