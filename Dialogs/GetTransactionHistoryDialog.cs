using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;
using ChatBot.Utils;
using ChatBot.Dtos;
using System.Collections.Generic;
using Newtonsoft.Json;
using ChatBot.Services.Interfaces;

namespace ChatBot.Dialogs
{
    public class GetTransactionHistoryDialog : CancelAndHelpDialog
    {
        private const string AdaptivePromptId = "adaptive";
        private readonly IAccountService _accountService;

        public GetTransactionHistoryDialog(IAccountService accountService)
        : base(nameof(OpenAccounDialog))
        {
            _accountService = accountService;
            AddDialog(new AdaptiveCardPrompt(AdaptivePromptId));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                FormStepAsync,
                ConfirmStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> FormStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var cardAttachment = AdaptiveCardHelper.CreateAdaptiveCardAttachment("AccountOpeningCard");

            var opts = new PromptOptions
            {
                Prompt = new Activity
                {
                    Attachments = new List<Attachment>() {
                            cardAttachment
                        },
                    Type = ActivityTypes.Message,
                    Text = "Please fill this form to open your account",
                }
            };

            // Prompt the user with the HeroCard
            return await stepContext.PromptAsync(AdaptivePromptId, opts, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var accountInfo = (OpenAccountDto)stepContext.Options;
            accountInfo = JsonConvert.DeserializeObject<OpenAccountDto>(stepContext.Result.ToString());
            stepContext.Values["AccountInfo"] = accountInfo;

            var messageText = $"Thank you for providing your data.\n\n" +
                  $"First Name: {accountInfo.FirstName}\n\n" +
                  $"Last Name: {accountInfo.LastName}\n\n" +
                  $"Other Name: {accountInfo.OtherName}\n\n" +
                  $"Email: {accountInfo.Email}\n\n" +
                  $"Phone Number: {accountInfo.PhoneNumber}\n\n" +
                  $"BVN Number: {accountInfo.BVNNumber}\n\n" +
                  $"NIN: {accountInfo.NIN}\n\n" +
                  $"Address: {accountInfo.Address}\n\n" +
                  $"Date of Birth: {accountInfo.DateOfBirth.ToString("dd-MM-yyyy")}\n\n" +
                  $"Account Type: {accountInfo.AccountType}\n\n" +
                  $"Please confirm your details above. Is this correct?";

            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                if ((bool)stepContext.Result != false)
                {
                    var accountInfo = (OpenAccountDto)stepContext.Values["AccountInfo"];

                    Console.WriteLine(accountInfo.Address);

                    var openAccountDto = new OpenAccountDto
                    {
                        FirstName = accountInfo.FirstName,
                        LastName = accountInfo.LastName,
                        OtherName = accountInfo.OtherName,
                        Email = accountInfo.Email,
                        PhoneNumber = accountInfo.PhoneNumber,
                        Address = accountInfo.Address,
                        BVNNumber = accountInfo.BVNNumber,
                        NIN = accountInfo.NIN,
                        DateOfBirth = accountInfo.DateOfBirth,
                        AccountType = accountInfo.AccountType
                    };

                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Account Opening in progress..."), cancellationToken);
                    var account = await _accountService.OpenAccountAsync(accountInfo);
                    var response = $"Your Tier 1 {account.AccountType} Account Opened Successfully, Your new Account number is {account.AccountNumber}. Remember, Tier 1 accounts have certain transaction and balance limits. To fully activate your account, please visit our nearest branch for KYC verification.";
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(response), cancellationToken);
                    return await stepContext.EndDialogAsync(accountInfo, cancellationToken);
                }

                var promptMessage = "Kindly repopulate the form with your details?";
                return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
            }
            catch (Exception)
            {
                var promptMessage = "Kindly repopulate the form with your details?";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Unable to Open Account,Something went Wrong, Please try again"), cancellationToken);
                return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
            }
        }
    }
}



