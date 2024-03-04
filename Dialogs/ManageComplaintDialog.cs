using ChatBot.Database.Models;
using ChatBot.Models;
using ChatBot.Resources;
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
        private readonly MessagePrompts _messages;

        public ManageComplaintDialog(
            LogComplaintDialog logComplaintDialog,
            TrackComplaintDialog trackComplaintDialog,
            ICustomerService customerService,
            AuthDialog authDialog,
            UserState userState,
            MessagePrompts messages

        )
     : base(nameof(ManageComplaintDialog))
        {
            _customerService = customerService;
            _messages = messages;
            _accountInfoAccessor = userState.CreateProperty<Account>("Account");
            AddDialog(authDialog);
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(trackComplaintDialog);
            AddDialog(logComplaintDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AuthenticateUserAsync,
                ConfirmAuthenticationAsync,
                SelectOptionAsync,
                RedirectAsync,
                AskForFurtherManagementStepAsync,
                FinalStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AuthenticateUserAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text(_messages.GetRandomMessage(_messages.ManageComplaintAuthSentences));
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            return await stepContext.BeginDialogAsync(nameof(AuthDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmAuthenticationAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var account = await _accountInfoAccessor.GetAsync(stepContext.Context, () => null, cancellationToken);
            stepContext.Values["Account"] = account;
            Customer customer = await _customerService.GetCustomerInfoAsync(account.CustomerId);

            var reply = MessageFactory.Text($"Yeap! **{customer.FirstName}** I found your account\n\n" +
                $"Lets move on");
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            return await stepContext.NextAsync(reply, cancellationToken);
        }

        private async Task<DialogTurnResult> SelectOptionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var card = new HeroCard
            {
                Text = _messages.GetRandomMessage(_messages.ManageComplaintSelectOptionsSentences) ,
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.PostBack, title: "Make A New Complaint", value: nameof(BankOperationIntent.LogComplaint)),
                    new CardAction(ActionTypes.PostBack, title: "Track Complaint Status", value: nameof(BankOperationIntent.TrackComplaintStatus)),
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
                case nameof(BankOperationIntent.LogComplaint):
                    return await stepContext.BeginDialogAsync(nameof(LogComplaintDialog), null, cancellationToken);

                case nameof(BankOperationIntent.TrackComplaintStatus):
                    return await stepContext.BeginDialogAsync(nameof(TrackComplaintDialog), null, cancellationToken);

                default:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText = _messages.GetRandomMessage(_messages.DidNotUnderstandSentences);
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> AskForFurtherManagementStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var messageText = _messages.GetRandomMessage(_messages.ManageFurtherComplaintSentences);
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result != false)
            {
                return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
            }

            var messageText = _messages.GetRandomMessage(_messages.ManageComplaintGratitudeMessages);
            var endMessage = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
            await stepContext.Context.SendActivityAsync(endMessage, cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        




    }
}

