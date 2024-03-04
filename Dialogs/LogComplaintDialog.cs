﻿using ChatBot.Database.Models;
using ChatBot.Dtos;
using ChatBot.Services.Interfaces;
using ChatBot.Utils;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ITransactionService _transactionService;

        public LogComplaintDialog(
            IAccountService accountService,
            ICustomerService customerService,
            IComplaintService complaintService,
            ITransactionService transactionService,
            AuthDialog authDialog,
            UserState userState
        )
     : base(nameof(LogComplaintDialog))
        {
            _accountService = accountService;
            _customerService = customerService;
            _complaintService = complaintService;
            _transactionService = transactionService;
            _accountInfoAccessor = userState.CreateProperty<Account>("Account");
            AddDialog(authDialog);
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new AdaptiveCardPrompt(AdaptivePromptId));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
        
        GetTransactionRefAsync,
        StoreTransactionRefeAsync,
        DescribeComplaintAsync2,
        StoreDescription2Async,
        SubmitComplaint3Async,
            }));

            InitialDialogId = nameof(WaterfallDialog);


        }

        private async Task<DialogTurnResult> GetTransactionRefAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("Can I have the Transaction Reference number? The one starting with 'TRX...'");

            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            return Dialog.EndOfTurn;
        }
        private async Task<DialogTurnResult> StoreTransactionRefeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Account account = await _accountInfoAccessor.GetAsync(stepContext.Context, () => null, cancellationToken);
            stepContext.Values["Account"] = account;

            string transactionRef = (string)stepContext.Result;
            transactionRef = transactionRef.Trim().ToUpper();
            stepContext.Values["transactionRef"] = transactionRef;
            Transaction transaction;

            try
            {
                List<Transaction> transactions = await _transactionService.GetTransactionsByReferenceAsync(account.Id, transactionRef);
                transaction = transactions.FirstOrDefault();
                stepContext.Values["transaction"] = transaction;

                string status = "";
                switch (transaction.Status)
                {
                    case (TransactionStatus)0:
                        status += "is pending";
                        break;
                    case (TransactionStatus)1:
                        status += "went through successfully";
                        break;
                    case (TransactionStatus)2:
                        status += "failed";
                        break;

                    case (TransactionStatus)3:
                        status += "has been reversed";
                        break;

                }
                string successMessage1 = $"Hmmm... I can see that on {transaction.CreatedAt.ToLongDateString()}, You made a {Enum.GetName(typeof(TransactionType), transaction.TransactionType)} of NGN{transaction.Amount:N2} using the {Enum.GetName(typeof(TransactionChannel), transaction.Channel)} to {transaction.RecipientName} with account number {transaction.RecipientAccountNumber} at {transaction.RecipientBankName}.";
                string successMessage2 = $"From my end, I can see that the transaction {status}. Is this the transaction you would like to make a complaint about?";

                var message = MessageFactory.Text(transactions != null
                ? $"{successMessage1}\n\n {successMessage2}"
                : "I'm sorry, no transactions exist with that reference.");

                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = message }, cancellationToken);
            }
            catch (Exception ex)
            {
                string message = $"I'm having a hard time finding the transaction...{ex.Message}";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
                return await stepContext.ReplaceDialogAsync(InitialDialogId, message, cancellationToken);
            }

        }
        private async Task<DialogTurnResult> DescribeComplaintAsync2(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var confirmed = (bool)stepContext.Result;
            if (confirmed)
            {
                var promptMessage = MessageFactory.Text("Before I log your complaint, please tell me, what challenges did you face with this transaction.");
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }
            else
            {
                string message = $"Oops! Sorry about that. Let's try another transaction.";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
                return await stepContext.ReplaceDialogAsync(InitialDialogId, message, cancellationToken);
            }
        }
        private async Task<DialogTurnResult> StoreDescription2Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var description = (string)stepContext.Result;
            stepContext.Values["description"] = description;
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I'm really sorry you had to experience any difficulties at all."), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Please hold on while I log your complaint..."), cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);
        }
        private async Task<DialogTurnResult> SubmitComplaint3Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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



            await stepContext.Context.SendActivityAsync("Your complaint has been successfully submitted. Our support team will get back to you shortly.\n\n" +
                $"You can track the status of your complaint with your Complaint Number: {complaint.ComplaintNo}.\n\n" +
                $"Until it is resolved, the status remainis {complaint.ComplaintStatus}");



            // End the dialog
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }



    }


}


//3121539729
//TRX2024022603155642
//comp-1914079465