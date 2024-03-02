using ChatBot.Database.Models;
using ChatBot.Dtos;
using ChatBot.Services.Interfaces;
using ChatBot.Utils;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBot.Dialogs
{
    public class LogComplaintDialog : CancelAndHelpDialog
    {
        private const string AdaptivePromptId = "adaptive";
        private readonly IStatePropertyAccessor<Account> _accountInfoAccessor;
        private readonly IAccountService _accountService;
        private readonly ICustomerService _customerService;
        private readonly IComplaintService _complaintService;
        public LogComplaintDialog(
            IAccountService accountService,
            ICustomerService customerService,
            IComplaintService complaintService,
            AuthDialog authDialog,
            UserState userState
        )
     : base(nameof(LogComplaintDialog))
        {
            _accountService = accountService;
            _customerService = customerService;
            _complaintService = complaintService;
            _accountInfoAccessor = userState.CreateProperty<Account>("Account");
            AddDialog(authDialog);
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new AttachmentPrompt(nameof(AttachmentPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new AdaptiveCardPrompt(AdaptivePromptId));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
        AuthenticateUserAsync,
        DisplayComplaintFormAsync,
        ConfirmSubmission1Async,
        SubmitComplaint1Async,
        /*SelectComplaintCategory,
        StoreCategory,
        CheckForTransactionPlatform,
        StorePlatform,
        DescribeComplaint,
        StoreDescription,
        GetTransactionDateAsync,
        GetTransactionAmount,
        ConfirmSubmission2Async,
        SubmitComplaint2Async,*/
            }));

            InitialDialogId = nameof(WaterfallDialog);


        }

        private async Task<DialogTurnResult> AuthenticateUserAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(AuthDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> DisplayComplaintFormAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var cardAttachment = AdaptiveCardHelper.CreateAdaptiveCardAttachment("ComplaintFormCard");

            var opts = new PromptOptions
            {
                Prompt = new Activity
                {
                    Attachments = new List<Attachment>() {
                            cardAttachment
                        },
                    Type = ActivityTypes.Message,
                    Text = "Lets log your complaint!",
                }
            };

            return await stepContext.PromptAsync(AdaptivePromptId, opts, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmSubmission1Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var complaint = (LogComplaintDto)stepContext.Options;
            complaint = JsonConvert.DeserializeObject<LogComplaintDto>(stepContext.Result.ToString());
            stepContext.Values["Complaint"] = complaint;

            var confirmationMessage = $"Thank you for providing your data.\n\n" +
                                       $"Category: {complaint.Category}\n\n" +
                                       $"Platform: {complaint.Platform}\n\n" +
                                       $"Date: {complaint.Date}\n\n" +
                                       $"Ref: {complaint.Ref}\n\n" +
                                       $"Description: {complaint.Description}\n\n" +
                                       $"Amount: {complaint.Amount}\n\n" +
                                       $"Please confirm your details above. Is this correct?";

            var promptMessage = MessageFactory.Text(confirmationMessage);
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> SelectComplaintCategory(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["AccountNumber"] = (string)stepContext.Result;

            var reply = MessageFactory.Text("What category of complaint would you like to make?");

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
        {
            new CardAction() { Title = "Money Transfer", Type = ActionTypes.ImBack, Value = "Money Transfer" },
            new CardAction() { Title = "Bill Payment", Type = ActionTypes.ImBack, Value = "Bill Payment" },
            new CardAction() { Title = "Airtime Purchase", Type = ActionTypes.ImBack, Value = "Airtime Purchase" },
            new CardAction() { Title = "Withdrawal", Type = ActionTypes.ImBack, Value = "Withdrawal" },
            new CardAction() { Title = "Others", Type = ActionTypes.ImBack, Value = "Others" },
        },
            };

            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            return Dialog.EndOfTurn;
        }

        private async Task<DialogTurnResult> StoreCategory(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var category = (string)stepContext.Result;
            stepContext.Values["category"] = category;
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> CheckForTransactionPlatform(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            string category = (string)stepContext.Values["category"];

            if (category == "Bill Payment" || category == "Money Transfer")
            {
                var reply = MessageFactory.Text("What platform did you use fior this transaction?");

                reply.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
        {
            new CardAction() { Title = "POS", Type = ActionTypes.ImBack, Value = "POS" },
            new CardAction() { Title = "ATM", Type = ActionTypes.ImBack, Value = "ATM" },
            new CardAction() { Title = "Web", Type = ActionTypes.ImBack, Value = "Web" },
            new CardAction() { Title = "Chat Banking", Type = ActionTypes.ImBack, Value = "Chat Banking" },
            new CardAction() { Title = "Internet Banking", Type = ActionTypes.ImBack, Value = "Internet Banking" },
            new CardAction() { Title = "Mobile Banking", Type = ActionTypes.ImBack, Value = "Mobile Banking" },
        },
                };

                await stepContext.Context.SendActivityAsync(reply, cancellationToken);

                return Dialog.EndOfTurn;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> StorePlatform(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var platform = "";
            string category = (string)stepContext.Values["category"];

            if (stepContext.Result != null && (string)stepContext.Result != category)
            {
                platform = (string)stepContext.Result;
            }

            stepContext.Values["platform"] = platform;
            return await stepContext.NextAsync(null, cancellationToken);
        }


        private async Task<DialogTurnResult> GetTransactionDateAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var cardAttachment = AdaptiveCardHelper.CreateAdaptiveCardAttachment("DateCard");

            var opts = new PromptOptions
            {
                Prompt = new Activity
                {
                    Attachments = new List<Attachment>() {
                            cardAttachment
                        },
                    Type = ActivityTypes.Message,
                    Text = "When did you carry out this transaction?!",
                }
            };

            return await stepContext.PromptAsync(AdaptivePromptId, opts, cancellationToken);
        }
        
        private async Task<DialogTurnResult> GetTransactionAmount(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var date = stepContext.Result;
            stepContext.Values["date"] = date;

            var cardAttachment = AdaptiveCardHelper.CreateAdaptiveCardAttachment("AmountCard");

            var opts = new PromptOptions
            {
                Prompt = new Activity
                {
                    Attachments = new List<Attachment>() {
                            cardAttachment
                        },
                    Type = ActivityTypes.Message,
                    Text = "When did you carry out this transaction?!",
                }
            };

            return await stepContext.PromptAsync(AdaptivePromptId, opts, cancellationToken);


        }



        private async Task<DialogTurnResult> DescribeComplaint(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptMessage = MessageFactory.Text("Please describe the issue you're experiencing.");
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> StoreDescription(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var moreDescription = (string)stepContext.Result;
            stepContext.Values["moreDescription"] = moreDescription;
            return await stepContext.NextAsync(null, cancellationToken);
        }
        
        private async Task<DialogTurnResult> ConfirmSubmission2Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var amount = stepContext.Result;
            var date = (object)stepContext.Values["date"];
            string category = (string)stepContext.Values["category"];
            string platform = stepContext.Values.ContainsKey("platform") ? (string)stepContext.Values["platform"] : "";
            string moreDescription = (string)stepContext.Values["moreDescription"];

            var confirmationMessage = $"Please confirm that the following details are correct:\n\n" +
                                        $"Complaint Description: {moreDescription} \n\n" +
                                        $"Category: {category}\n\n" +
                                        $"{(platform.Length > 0 ? $"Platform: {platform}\n\n" : "")}" +
                                        $"Transaction Date: {date} \n\n" +
                                        $"Amount: {amount} \n\n";

            var promptMessage = MessageFactory.Text(confirmationMessage);
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }




        private async Task<DialogTurnResult> SubmitComplaint2Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Submit the complaint to the backend system or support team
            var confirmed = (bool)stepContext.Result;
            if (confirmed)
            {
                // Submit the complaint and provide feedback to the user
                await stepContext.Context.SendActivityAsync("Your complaint has been successfully submitted. Our support team will get back to you shortly.");
            }
            else
            {
                // If the user didn't confirm, provide appropriate feedback
                await stepContext.Context.SendActivityAsync("Your complaint submission has been cancelled.");
            }

            // End the dialog
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> SubmitComplaint1Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                // Check if the user has confirmed the submission
                if ((bool)stepContext.Result != false)
                {
                    // Check if the complaint details exist in stepContext.Values
                    if (stepContext.Values.TryGetValue("Complaint", out var complaintObj) && complaintObj is LogComplaintDto complaintInfo)
                    {
                        // Log the complaint
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Hold on, I'm logging your complaint..."), cancellationToken);
                        var complaint = await _complaintService.LogComplaintAsync(complaintInfo);
                        var response = $"Your complaint has been logged successfully. Your new Complaint ID is '{complaint.ComplaintId}'.";
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text(response), cancellationToken);
                        return await stepContext.EndDialogAsync(complaintInfo, cancellationToken);
                    }
                    else
                    {
                        // Handle case where complaint details are missing
                        var promptMessage = "Kindly fill the form again with the right details";
                        return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
                    }
                }
                else
                {
                    // Handle case where user didn't confirm the submission
                    var promptMessage = "Kindly fill the form again with the right details";
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                // Handle specific exceptions and provide appropriate feedback
                var errorMessage = $"An error occurred while processing your complaint. Please try again later. {ex.Message}";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(errorMessage), cancellationToken);
                return await stepContext.CancelAllDialogsAsync(cancellationToken);
            }
        }

    }


}


