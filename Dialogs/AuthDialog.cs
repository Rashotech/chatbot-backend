using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;
using ChatBot.Services.Interfaces;
using System.Text.RegularExpressions;
using ChatBot.Database.Models;
using System.Collections.Generic;

namespace ChatBot.Dialogs
{
    public class AuthDialog : CancelAndHelpDialog
    {
        private readonly IStatePropertyAccessor<Account> _accountInfoAccessor;
        private const string AdaptivePromptId = "adaptive";
        private readonly IAccountService _accountService;
        private readonly ICustomerService _customerService;
        private readonly string AccountNumberDlgId = "AccountNumberDlgId";
        private readonly string SendOtpDlgId = "SendOtpDlgId";
        private readonly string ConfirmOtpDlgId = "ConfirmOtpDlgId";
        private readonly string DataNoticeDlgId = "DataNoticeDlgId";
        private bool hasAcceptedDataNotice = false;

        public AuthDialog(IAccountService accountService, ICustomerService customerService, UserState userState)
        : base(nameof(AuthDialog))
        {
            _accountInfoAccessor = userState.CreateProperty<Account>("Account");
            _accountService = accountService;
            _customerService = customerService;
            AddDialog(new TextPrompt(AccountNumberDlgId, AccountNumberValidator));
            AddDialog(new TextPrompt(DataNoticeDlgId));
            AddDialog(new TextPrompt(SendOtpDlgId, OtpValidator));
            AddDialog(new TextPrompt(ConfirmOtpDlgId));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                RequestAccountNumberStepAsync,
                DataNoticeStepAsync,    
                AcknowledgeDataNoticeStepAsync,
                ConfirmAccountNumberStepAsync,
                SendOtpStepAsync,
                ConfirmOtpStepAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> RequestAccountNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Please enter your 10 digit Account Number."),
                RetryPrompt = MessageFactory.Text("Invalid Account Number, Please enter your 10 digit Account Number."),
            };

            return await stepContext.PromptAsync(AccountNumberDlgId, promptOptions, cancellationToken);
        }


        private async Task<DialogTurnResult> DataNoticeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["AccountNumber"] = (string)stepContext.Result;

            if (hasAcceptedDataNotice == false)
            {
                var reply = MessageFactory.Text(
                 "**Data Protection Notice**" +
                 "\n\nTo provide you with our products and services, we need to collect, record, use, share and store personal and financial information about you (“Information”). Your Information may include Personal Data and Sensitive Personal Data as defined in the Nigeria Data Protection Regulation 2019 (“NDPR”) (as may be amended, replaced, or re-enacted from time to time) and any other law or regulation governing Data Protection."
                 );

                reply.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                    {
                        new CardAction() { Title = "Accept", Type = ActionTypes.ImBack, Value = "Accept" },
                        new CardAction() { Title = "Decline", Type = ActionTypes.ImBack, Value = "Decline" },
                    },
                };
                await stepContext.Context.SendActivityAsync(reply, cancellationToken);

                return Dialog.EndOfTurn;
            }

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> AcknowledgeDataNoticeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            hasAcceptedDataNotice = (string)stepContext.Result == "Accept";
            if (hasAcceptedDataNotice)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you for accepting the data consent."), cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Validating account number, Please wait for a moment.."), cancellationToken);
                return await stepContext.NextAsync();
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("You can’t proceed further as you have rejected data consent."), cancellationToken);
                return await stepContext.CancelAllDialogsAsync();
            }
        }

        private async Task<DialogTurnResult> ConfirmAccountNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                var accountNumber = (string)stepContext.Values["AccountNumber"];
                var account = await _accountService.GetAccountByAccountNumber(accountNumber);
                var customer = await _customerService.GetCustomerInfoAsync(account.Id);

                await _accountInfoAccessor.SetAsync(stepContext.Context, account, cancellationToken);

                stepContext.Values["Customer"] = customer;
                stepContext.Values["Account"] = account;

                var messageText = $"Are you {customer.FirstName}?";
                var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);

                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }
            catch (Exception ex)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(ex.Message), cancellationToken);
                return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> SendOtpStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result != false)
            {
                var customer = (Customer)stepContext.Values["Customer"];
                var promptText = $"Kindly enter OTP sent to your phone number ending with {customer.PhoneNumber.Substring(customer.PhoneNumber.Length - 4)}";

                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(promptText),
                    RetryPrompt = MessageFactory.Text("Wrong OTP, Kindly input Correct Otp."),
                };

                return await stepContext.PromptAsync(SendOtpDlgId, promptOptions, cancellationToken);
            }

            return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmOtpStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var otp = (string)stepContext.Result;
            var isOtpValid = _accountService.ValidateOtp(otp);
            if(!isOtpValid)
            {
                var promptText = "Wrong OTP, Kindly input Correct Otp";
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(promptText),
                };
                return await stepContext.PromptAsync(ConfirmOtpDlgId, promptOptions, cancellationToken);
            }

            return await stepContext.EndDialogAsync();
        }

        private static Task<bool> AccountNumberValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(Regex.Match(promptContext.Context.Activity.Text, @"\d+").Value != "" &&
                 promptContext.Context.Activity.Text.Length == 10);
        }

        private Task<bool> OtpValidator(PromptValidatorContext<string> promptcontext, CancellationToken cancellationtoken)
        {
            var otp = promptcontext.Recognized.Value;
            var isOtpValid = _accountService.ValidateOtp(otp);
            if (!isOtpValid) return Task.FromResult(false);
            return Task.FromResult(true);
        }
    }
}





