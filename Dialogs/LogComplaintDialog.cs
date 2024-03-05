using ChatBot.Database.Models;
using ChatBot.Dtos;
using ChatBot.Services.Interfaces;
using ChatBot.Utils;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
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
        private readonly IComplaintService _complaintService;
        private readonly ITransactionService _transactionService;
        private readonly MessagePrompts _messages;

        public LogComplaintDialog(
            IComplaintService complaintService,
            ITransactionService transactionService,
            AuthDialog authDialog,
            UserState userState,
            MessagePrompts messages

        )
     : base(nameof(LogComplaintDialog))
        {
            _complaintService = complaintService;
            _transactionService = transactionService;
            _messages = messages;
            _accountInfoAccessor = userState.CreateProperty<Account>("Account");
            AddDialog(authDialog);
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new AdaptiveCardPrompt(AdaptivePromptId));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {

        GetTransactionRefAsync,
        StoreTransactionRefeAsync,
        DescribeComplaintAsync,
        StoreDescriptionAsync,
        UserWaitAsync,
        SubmitComplaintAsync,
        AskForFurtherComplaintAsync,
        FinalStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
            //this.messages = messages;
        }

        private async Task<DialogTurnResult> GetTransactionRefAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text(_messages.GetRandomMessage(_messages.TransactionRefRequestsMessages));

            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            return Dialog.EndOfTurn;
        }

        private async Task<DialogTurnResult> StoreTransactionRefeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Account account = await _accountInfoAccessor.GetAsync(stepContext.Context, () => null, cancellationToken);
            stepContext.Values["Account"] = account;
            bool isFound = true;

            string transactionRef = (string)stepContext.Result;
            transactionRef = transactionRef.Trim().ToUpper();
            stepContext.Values["transactionRef"] = transactionRef;

            try
            {
                var transaction = await _transactionService.GetTransactionByReferenceAsync(account.Id, transactionRef) != null ? await _transactionService.GetTransactionByReferenceAsync(account.Id, transactionRef) : null;
                stepContext.Values["transaction"] = transaction;

                string status = "";
                switch (transaction.Status.ToString())
                {
                    case nameof(TransactionStatus.Pending):
                        status += "pending";
                        break;
                    case nameof(TransactionStatus.Successful):
                        status += "successful";
                        break;
                    case nameof(TransactionStatus.Failed):
                        status += "failed";
                        break;

                    case nameof(TransactionStatus.Reversed):
                        status += "reversed";
                        break;

                }

                List<string> transactionDetailsMessages = new List<string>
                {
                    $"Hmmm... According to my records, on **{transaction.CreatedAt.ToLongDateString()}**, there was a **{Enum.GetName(typeof(TransactionType), transaction.TransactionType)}** transaction of **NGN{(transaction.Amount) / 100:N2}** made using the **{Enum.GetName(typeof(TransactionChannel), transaction.Channel)}** channel to **{transaction.RecipientName}** with account number **{transaction.RecipientAccountNumber}** at **{transaction.RecipientBankName}**. " +
                    $"From what I can tell, the transaction appears to be **{status}**. Does this transaction match the one you'd like to file a complaint about?",

                    $"Hmm... Based on my logs, it seems that on **{transaction.CreatedAt.ToLongDateString()}**, there was a **{Enum.GetName(typeof(TransactionType), transaction.TransactionType)}** transaction worth **NGN{(transaction.Amount) / 100:N2}** using the **{Enum.GetName(typeof(TransactionChannel), transaction.Channel)}** channel to **{transaction.RecipientName}** with account number **{transaction.RecipientAccountNumber}** at **{transaction.RecipientBankName}**. " +
                    $"From my end, it looks like the transaction is **{status}**. Is this the transaction you're referring to for your complaint?",

                    $"Hmm... Let me check my records. " +
                    $"" +
                    $"It appears that on **{transaction.CreatedAt.ToLongDateString()}**, there was a **{Enum.GetName(typeof(TransactionType), transaction.TransactionType)}** transaction totaling **NGN{(transaction.Amount) / 100:N2}** using the **{Enum.GetName(typeof(TransactionChannel), transaction.Channel)}** channel to **{transaction.RecipientName}** with account number **{transaction.RecipientAccountNumber}** at **{transaction.RecipientBankName}**. " +
                    $"As far as I can see, the transaction is currently marked as **{status}**. Is this the transaction you'd like to address with a complaint?",

                    $"Sure, let me pull up the details for you. " +
                    $"" +
                    $"It seems that on **{transaction.CreatedAt.ToLongDateString()}**, there was a **{Enum.GetName(typeof(TransactionType), transaction.TransactionType)}** transaction valued at **NGN{(transaction.Amount) / 100:N2}** using the **{Enum.GetName(typeof(TransactionChannel), transaction.Channel)}** platform to **{transaction.RecipientName}** with account number **{transaction.RecipientAccountNumber}** at **{transaction.RecipientBankName}**. " +
                    $"As per my records, the transaction is currently marked as **{status}**. Does this match the transaction you'd like to file a complaint about?",

                    $"Certainly! Let me fetch the details. " +
                    $"" +
                    $"It appears that on **{transaction.CreatedAt.ToLongDateString()}**, there was a **{Enum.GetName(typeof(TransactionType), transaction.TransactionType)}** transaction worth **NGN{(transaction.Amount) / 100:N2}** made using the **{Enum.GetName(typeof(TransactionChannel), transaction.Channel)}** channel to **{transaction.RecipientName}** with account number **{transaction.RecipientAccountNumber}** at **{transaction.RecipientBankName}**. " +
                    $"From what I see, the transaction status is **{status}**. Is this the transaction you're referring to for your complaint?",

                    $"Absolutely! Let me retrieve the details for you. " +
                    $"" +
                    $"It seems that on **{transaction.CreatedAt.ToLongDateString()}**, there was a **{Enum.GetName(typeof(TransactionType), transaction.TransactionType)}** transaction totaling **NGN{(transaction.Amount) / 100:N2}** using the **{Enum.GetName(typeof(TransactionChannel), transaction.Channel)}** channel to **{transaction.RecipientName}** with account number **{transaction.RecipientAccountNumber}** at **{transaction.RecipientBankName}**. " +
                    $"Based on my records, the transaction is currently **{status}**. Does this match the transaction you'd like to address with a complaint?",

                    $"Of course! Let me gather the information." +
                    $"" +
                    $"It looks like on **{transaction.CreatedAt.ToLongDateString()}**, there was a **{Enum.GetName(typeof(TransactionType), transaction.TransactionType)}** transaction valued at **NGN{(transaction.Amount) / 100:N2}** using the **{Enum.GetName(typeof(TransactionChannel), transaction.Channel)}** platform to **{transaction.RecipientName}** with account number **{transaction.RecipientAccountNumber}** at **{transaction.RecipientBankName}**. " +
                    $"According to my data, the transaction is currently labeled as **{status}**. Is this the transaction you want to discuss with a complaint?",

                    $"Certainly! Let me find the details for you. " +
                    $"" +
                    $"It seems that on **{transaction.CreatedAt.ToLongDateString()}**, there was a **{Enum.GetName(typeof(TransactionType), transaction.TransactionType)}** transaction worth **NGN{(transaction.Amount) / 100:N2}** made using the **{Enum.GetName(typeof(TransactionChannel), transaction.Channel)}** channel to **{transaction.RecipientName}** with account number **{transaction.RecipientAccountNumber}** at **{transaction.RecipientBankName}**. " +
                    $"As per my records, the transaction status is **{status}**. Does this match the transaction you'd like to file a complaint about?"

                };
                string response = _messages.GetRandomMessage(transactionDetailsMessages);
                
                var message = MessageFactory.Text(transaction != null
                ? response
                : "I'm sorry, no transactions exist with that reference.");
                stepContext.Values["isFound"] = isFound;

                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = message }, cancellationToken);
            }
            catch (Exception)
            {
                isFound = false;
                stepContext.Values["isFound"] = isFound;
                var message = MessageFactory.Text(_messages.GetRandomMessage(_messages.TransactionNotFoundMessages));

                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = message }, cancellationToken);

            }

        }

        private async Task<DialogTurnResult> DescribeComplaintAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var isFound = (bool)stepContext.Values["isFound"];
            if (isFound == true)
            {
                var confirmed = (bool)stepContext.Result;
                if (confirmed)
                {
                    var promptMessage = MessageFactory.Text(_messages.GetRandomMessage(_messages.LogComplaintChallengesMessages));
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
                }
                else
                {
                    string message = _messages.GetRandomMessage(_messages.RetryTransactionMessages);
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, message, cancellationToken);
                }
            }
            else
            {
                var confirmed = (bool)stepContext.Result;
                if (confirmed)
                {
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
                }
                else
                {
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                }
            }
        }

        private async Task<DialogTurnResult> StoreDescriptionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var description = (string)stepContext.Result;
            stepContext.Values["description"] = description;
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> UserWaitAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(_messages.GetRandomMessage(_messages.LogComplaintApologyMessages)), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(_messages.GetRandomMessage(_messages.LogComplaintWaitMessages)), cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);
        }

            private async Task<DialogTurnResult> SubmitComplaintAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Account account = (Account)stepContext.Values["Account"];
            Transaction transaction = (Transaction)stepContext.Values["transaction"];
            int amount = transaction.Amount;
            DateTime date = transaction.CreatedAt.Date;
            string category = Enum.GetName(typeof(TransactionType), transaction.TransactionType);
            Channel channel = (Channel)transaction.Channel;
            string transactionRef = transaction.TransactionReference;
            string description = (string)stepContext.Values["description"];


            var logComplaintDto = new LogComplaintDto
            {
                Channel = channel,
                Amount = amount,
                Description = description,
                Date = date,
                Category = category,
                AccountId = account.Id,
                TransactionRef = transactionRef

            };

            var complaint = await _complaintService.LogComplaintAsync(logComplaintDto);
            var message = new List<string>
            {
                $"🎉 Congratulations! Your complaint has been successfully submitted. Our team will review it shortly and get back to you soon. 🚀  Your Complaint Number: {complaint.ComplaintNo}",
                $"📝 Your complaint has been received and is now in our system with the Complaint Number: {complaint.ComplaintNo}. You can track its progress at any time. 🔄",
                $"👍 Thank you for submitting your complaint. Your Complaint Number is {complaint.ComplaintNo}. Our team will work on resolving it promptly. 🛠️",
                $"👏 Well done! Your complaint has been logged successfully. Keep an eye on its progress with Complaint Number: {complaint.ComplaintNo}. 📈",
                $"📝 Your complaint has been registered with the Complaint Number: {complaint.ComplaintNo}. We'll keep you updated on its status. 🔄",
                $"🎉 Hooray! Your complaint has been successfully submitted. Our team is now on the case! 🕵️‍♂️  Your Complaint Number: {complaint.ComplaintNo}",
                $"📋 Thank you for bringing this to our attention. Your Complaint Number is {complaint.ComplaintNo}. We'll investigate and respond shortly. 🚀",
                $"📝 We've received your complaint with the Complaint Number: {complaint.ComplaintNo}. Expect updates from us soon. 🔄",
                $"🎉 Your complaint is now in our system with the Complaint Number: {complaint.ComplaintNo}. Our team will look into it shortly. 🛠️"
            };
            await stepContext.Context.SendActivityAsync(_messages.GetRandomMessage(message), cancellationToken: cancellationToken);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> AskForFurtherComplaintAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var messageText = _messages.GetRandomMessage(_messages.AdditionalComplaintPromptMessages);
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