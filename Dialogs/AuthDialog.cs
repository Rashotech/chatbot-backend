using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;
using ChatBot.Services.Interfaces;
using System.Text.RegularExpressions;
using ChatBot.Database.Models;

namespace ChatBot.Dialogs
{
    public class AuthDialog : CancelAndHelpDialog
    {
        private const string AdaptivePromptId = "adaptive";
        private readonly IAccountService _accountService;
        private readonly ICustomerService _customerService;
        private readonly string AccountNumberDlgId = "AccountNumberDlgId";
        private readonly string SendOtpDlgId = "SendOtpDlgId";
        private readonly string ConfirmOtpDlgId = "ConfirmOtpDlgId";

        public AuthDialog(IAccountService accountService, ICustomerService customerService)
        : base(nameof(AuthDialog))
        {
            _accountService = accountService;
            _customerService = customerService;
            AddDialog(new TextPrompt(AccountNumberDlgId, AccountNumberValidator));
            AddDialog(new TextPrompt(SendOtpDlgId, OtpValidator));
            AddDialog(new TextPrompt(ConfirmOtpDlgId));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                RequestAccountNumberStepAsync,
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

        private async Task<DialogTurnResult> ConfirmAccountNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                var accounNumber = stepContext.Result.ToString();
                var account = await _accountService.GetAccountByAccountNumber(accounNumber);
                var customer = await _customerService.GetCustomerInfoAsync(account.Id);

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

            return await stepContext.EndDialogAsync(stepContext, cancellationToken);
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





