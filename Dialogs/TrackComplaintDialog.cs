using ChatBot.Database.Models;
using ChatBot.Services.Interfaces;
using ChatBot.Utils;
using ChatBot.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBot.Dialogs
{
    public class TrackComplaintDialog : ComponentDialog
    {
        private const string AdaptivePromptId = "adaptive";
        private readonly IStatePropertyAccessor<Account> _accountInfoAccessor;
        private readonly IComplaintService _complaintService;
        private readonly MessagePrompts _messages;


        public TrackComplaintDialog(
            IComplaintService complaintService,
            AuthDialog authDialog,
            UserState userState,
            MessagePrompts messages
        )
     : base(nameof(TrackComplaintDialog))
        {
            _complaintService = complaintService;
            _messages = messages;
            _accountInfoAccessor = userState.CreateProperty<Account>("Account");
            AddDialog(authDialog);
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new AdaptiveCardPrompt(AdaptivePromptId));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
        GetComplaintNoAsync,
        StoreComplaintNoAsync,
        AskForFurtherComplaintAsync,
        FinalStepAsync,
        
            }));

            InitialDialogId = nameof(WaterfallDialog);


        }

        private async Task<DialogTurnResult> GetComplaintNoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text(_messages.GetRandomMessage(_messages.ComplaintNumberRequestMessages));

            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            return Dialog.EndOfTurn;
        }

        private async Task<DialogTurnResult> StoreComplaintNoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Account account = await _accountInfoAccessor.GetAsync(stepContext.Context, () => null, cancellationToken);
            var complaintNo = (string)stepContext.Result;
            complaintNo = complaintNo.ToUpper();
            try
            {
               var complaint = await _complaintService.GetComplaintByComplaintNo(account.Id, complaintNo);
               string apology = "";


                string message = complaint != null
                ? $"I found it!\n\n"
                : "I'm sorry, no complaints exist with that Number.";

                if(complaint != null)
                {
                    message += "\n\n" +
                                           $"Account Number: {account.AccountNumber}\n\n" +
                                           $"Category: {complaint.Category}\n\n" +
                                           $"Platform: {complaint.Channel}\n\n" +
                                           $"Date: {complaint.Date}\n\n" +
                                           $"Ref: {complaint.TransactionRef}\n\n" +
                                           $"Description: {complaint.Description}\n\n" +
                                           $"Amount: NGN{(complaint.Amount)/100:N2}\n\n" +
                                           $"Status: {Enum.GetName(typeof(Status), complaint.ComplaintStatus)}";


                    if (complaint.ComplaintStatus == 0)
                    {
                        apology += _messages.GetRandomMessage(_messages.ComplaintNotResolvedMessages);
                    }

                    else
                    {
                        apology += _messages.GetRandomMessage(_messages.IssueResolvedMessages);
                    }

                }

                await stepContext.Context.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(apology), cancellationToken);
            }
            catch (Exception ex)
            {
                string message = _messages.GetRandomMessage(_messages.ComplaintRetrievalErrorMessages);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(InitialDialogId), cancellationToken);
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> AskForFurtherComplaintAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var messageText =_messages.GetRandomMessage(_messages.AdditionalComplaintRequestMessages);
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result != false)
            {
                return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
            }

            var messageText = $"Okay...";
            var endMessage = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
            await stepContext.Context.SendActivityAsync(endMessage, cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}