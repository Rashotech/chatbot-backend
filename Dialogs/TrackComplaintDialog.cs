using ChatBot.Database.Models;
using ChatBot.Services.Interfaces;
using ChatBot.Utils;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
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


        public TrackComplaintDialog(
            IComplaintService complaintService,
            AuthDialog authDialog,
            UserState userState
        )
     : base(nameof(TrackComplaintDialog))
        {
            _complaintService = complaintService;
            _accountInfoAccessor = userState.CreateProperty<Account>("Account");
            AddDialog(authDialog);
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new AdaptiveCardPrompt(AdaptivePromptId));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
        GetComplaintNoAsync,
        StoreComplaintNoAsync,
        
            }));

            InitialDialogId = nameof(WaterfallDialog);


        }

        private async Task<DialogTurnResult> GetComplaintNoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("Can I have the Complaint Number? The one starting with 'COMP...'");

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
                ? $"I found it!"
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
                        apology += "I'm so sorry your complaint is yet to be resolved. I promise we're working on it as much as we can.";
                    }

                }

                await stepContext.Context.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(apology), cancellationToken);
            }
            catch (Exception ex)
            {
                string message = $"I'm having a hard time finding the Complaint...{ex.Message}\n\n" +
                    $"Don't worry, I'll log your complaints anyway.";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(InitialDialogId), cancellationToken);
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }
    }
}