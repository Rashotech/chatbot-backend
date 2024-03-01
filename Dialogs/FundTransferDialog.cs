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
using ChatBot.Utils;
using ChatBot.Models;
using ChatBot.Dtos;
using Newtonsoft.Json;

namespace ChatBot.Dialogs
{
    public class FundTransferDialog : CancelAndHelpDialog
    {
        private readonly IStatePropertyAccessor<Account> _accountInfoAccessor;
        private readonly IPaymentProvider _paymentProvider;
        private const string AdaptivePromptId = "adaptive";
        private readonly IAccountService _accountService;
        private readonly ITransactionService _transactionService;
        private readonly string TransactionAmountDlgId = "TransactionAmountDlgId";
        private readonly string PinDlgId = "PinDlgId";
        private readonly string RequestRecipientAccountNumberDlgId = "RequestRecipientAccountNumberDlgId";
        private readonly string NarrationDlgId = "NarrationDlgId";

        public FundTransferDialog(
            IAccountService accountService,
            ITransactionService transactionService,
            IPaymentProvider paymentProvider,
            UserState userState,
            AuthDialog authDialog
        )
        : base(nameof(FundTransferDialog))
        {
            _accountInfoAccessor = userState.CreateProperty<Account>("Account");
            _accountService = accountService;
            _transactionService = transactionService;
            _paymentProvider = paymentProvider;
            AddDialog(authDialog);
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new AdaptiveCardPrompt(AdaptivePromptId));
            AddDialog(new TextPrompt(RequestRecipientAccountNumberDlgId, TransactionAmountValidator));
            AddDialog(new TextPrompt(TransactionAmountDlgId, TransactionAmountValidator));
            AddDialog(new TextPrompt(NarrationDlgId));
            AddDialog(new TextPrompt(PinDlgId, PinValidator));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AuthenticationStepAsync,
                BankSelectionStepAsync,
                BankListSelectionStepAsync,
                SelectBankStepAsync,
                RequestRecipientAccountNumberAsync,
                InitiateResolveAccountNumberAsync,
                ConfirmAccountNumberStepAsync,
                TransactionAmountStepAsync,
                TransactionNarrationStepAsync,
                ConfirmTransactionDetailsStepAsync,
                RequestPinStepAsync,
                FundTransferStepAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AuthenticationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(AuthDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> BankSelectionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("Please confirm the type of transfer you want to initiate");

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "First Bank", Type = ActionTypes.PostBack, Value = "FIRST BANK" },
                    new CardAction() { Title = "Other bank", Type = ActionTypes.PostBack, Value = "OTHER BANK" },
                },
            };
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            return Dialog.EndOfTurn;
        }

        private async Task<DialogTurnResult> BankListSelectionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bankType = (string)stepContext.Result;
            if (bankType == "FIRST BANK")
            {
                var bank = new Bank()
                {
                    name = "First Bank of Nigeria",
                    code = "011"
                };
                stepContext.Values["bank"] = bank;
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
            else
            {
                var banks = await _paymentProvider.GetBankListAsync();
                banks.RemoveAll(item => item.code == "011");
                stepContext.Values["banks"] = banks;

                var cardAttachment = AdaptiveCardHelper.CreateAdaptiveCardAttachment("FundTransferCard", banks,
                    getTitle: bank => bank.name,
                    getValue: bank => bank.code
                );

                var opts = new PromptOptions
                {
                    Prompt = new Microsoft.Bot.Schema.Activity
                    {
                        Attachments = new List<Attachment>() {
                            cardAttachment
                        },
                        Type = ActivityTypes.Message,
                        Text = "Please select the bank from dropdown",
                    }
                };
                stepContext.Values["bank"] = null;

                return await stepContext.PromptAsync(AdaptivePromptId, opts, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> SelectBankStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                var bank = (Bank)stepContext.Values["bank"];

                if(bank == null)
                {
                    var result = (ExtractBankDto)stepContext.Options;
                    result = JsonConvert.DeserializeObject<ExtractBankDto>(stepContext.Result.ToString());
                    var banks = (List<Bank>)stepContext.Values["banks"];
                    var selectedBank = banks.Find(bank => bank.code == result.BankCode);
                    var bankData = new Bank()
                    {
                        name = selectedBank.name,
                        code = result.BankCode
                    };
                    stepContext.Values["bank"] = bankData;
                    var message = MessageFactory.Text($"Selected bank: {selectedBank.name}");
                    await stepContext.Context.SendActivityAsync(message, cancellationToken);
                }

                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            } catch(Exception ex)
            {
                var errorMessage = "Fetching of Destination Bank List failed, Please Try again later";
                errorMessage = ex.Message != "" ? ex.Message : errorMessage;
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(errorMessage), cancellationToken);
                return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> RequestRecipientAccountNumberAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptText = "Enter Recipient Account Number";

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text(promptText),
                RetryPrompt = MessageFactory.Text("Account Not Found"),
            };

            return await stepContext.PromptAsync(RequestRecipientAccountNumberDlgId, promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult>InitiateResolveAccountNumberAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var accountNumber = (string)stepContext.Result;
            var text = "Kindly wait while we validate the Recipient Account Number";

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(text), cancellationToken);

            return await stepContext.NextAsync(accountNumber, cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmAccountNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var errorMessage = "Unable to Resolve  Recipient Account Number. Please input correct account number and try again.";
            try
            {
                var accountNumber = (string)stepContext.Result;
                var bank = (Bank)stepContext.Values["bank"];
                var account = await _paymentProvider.ResolveAccountAsync(accountNumber, bank.code);

                if (account != null)
                {
                    var recipient = new RecipientDto()
                    {
                        RecipientAccountNumber = accountNumber,
                        RecipientName = account.account_name
                    };
                    stepContext.Values["recipient"] = recipient;

                    var text = $"Recipient Account Name: {account.account_name}";
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(text), cancellationToken);

                    var messageText = "Confirm?";
                    var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(errorMessage), cancellationToken);
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
                }
            }
            catch (Exception)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(errorMessage), cancellationToken);
                return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> TransactionAmountStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result != false)
            {
                var promptText = "Enter Amount to be transferred";

                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(promptText),
                    RetryPrompt = MessageFactory.Text("Invalid Amount"),
                };

                return await stepContext.PromptAsync(TransactionAmountDlgId, promptOptions, cancellationToken);
            }
            else
            {
                return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
            };
        }

        private async Task<DialogTurnResult> TransactionNarrationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var transactionAmount = (string)stepContext.Result;
            stepContext.Values["transactionAmount"] = transactionAmount;

            var promptText = "Enter Transaction Narration";

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text(promptText)
            };

            return await stepContext.PromptAsync(NarrationDlgId, promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmTransactionDetailsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var narration = (string)stepContext.Result;
            var transactionAmount = (string)stepContext.Values["transactionAmount"];
            var bank = (Bank)stepContext.Values["bank"];
            var recipient = (RecipientDto)stepContext.Values["recipient"];
            var account = await _accountInfoAccessor.GetAsync(stepContext.Context, () => null, cancellationToken);
            var amount = Math.Round(Decimal.Parse(transactionAmount), 2);

            var messageText = $"Please find the below transaction details:\n\n" +
                  $"Sender Account Number: {account.AccountNumber}\n\n" +
                  $"Recipient Account Number: {recipient.RecipientAccountNumber}\n\n" +
                  $"Recipient Account Name: {recipient.RecipientName}\n\n" +
                  $"Recipient Bank name: {bank.name}\n\n" +
                  $"Amount to be transferred(NGN): {amount}\n\n" +
                  $"Narration: {narration}\n\n" +
                  $"Please confirm your details above. Is this correct?";

            var fundTransferDto = new FundTransferDto()
            {
                AccountId = account.Id,
                Amount = amount,
                Narration = narration,
                RecipientAccountNumber = recipient.RecipientAccountNumber,
                RecipientBankCode = bank.code,
                RecipientName = recipient.RecipientName,
                RecipientBankName = bank.name
            };
            stepContext.Values["fundTransferDto"] = fundTransferDto;

            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> RequestPinStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptText = "Enter your ATM pin to complete this transaction";

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text(promptText),
                RetryPrompt = MessageFactory.Text("Invalid Pin, Kindly enter a valid Pin"),
            };

            return await stepContext.PromptAsync(PinDlgId, promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> FundTransferStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                var fundTransferDto = (FundTransferDto)stepContext.Values["fundTransferDto"];
                await _transactionService.FundTransfer(fundTransferDto);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Fund Transfer Successful"), cancellationToken);

                var account = await _accountInfoAccessor.GetAsync(stepContext.Context, () => null, cancellationToken);
                var balance = await _accountService.GetAccountBalanceAsync(account.Id);
                string accountBalance = $"{balance.Currency} {balance.Balance:N2}";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Your new balance is {accountBalance}"), cancellationToken);
            }
            catch (Exception ex)
            {
                var errorMessage = "Fund Transfer Failed, Please Try again later";
                errorMessage = ex.Message != "" ? ex.Message : errorMessage;
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(errorMessage), cancellationToken);
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private static Task<bool> AccountNumberValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(Regex.Match(promptContext.Context.Activity.Text, @"\d+").Value != "" &&
                promptContext.Context.Activity.Text.Length == 10);
        }

        private static Task<bool> TransactionAmountValidator(PromptValidatorContext<string> promptcontext, CancellationToken cancellationtoken)
        {
            var amount = promptcontext.Context.Activity.Text;
            var isValid = Regex.Match(amount, @"\d+").Value != "" && Decimal.Parse(amount) > 0;
            return Task.FromResult(isValid);
        }

        private async Task<bool> PinValidator(PromptValidatorContext<string> promptcontext, CancellationToken cancellationtoken)
        {
            var maxAttempts = 3;
            var attempts = promptcontext.AttemptCount;
            var pin = promptcontext.Recognized.Value;
            var isPinValid = _accountService.ValidatePin(pin);
            if (isPinValid == false)
            {
                var attemptsLeft = maxAttempts - attempts;
                if(attemptsLeft > 0)
                {
                    var text = MessageFactory.Text($"Incorrect Pin\n{attemptsLeft} Attempts left");
                    await promptcontext.Context.SendActivityAsync(text, cancellationtoken);
                    return false;
                }
                else
                {
                    var message = MessageFactory.Text("Too many failed attempts. Try again later. Type Quit to exit the bot");
                    await promptcontext.Context.SendActivityAsync(message, cancellationtoken);
                    return false;
                }
            } else
            {
                return true;
            }
        }
    }
}





