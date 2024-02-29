using ChatBot.Database.Models;
using ChatBot.Services;
using ChatBot.Services.Interfaces;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBot.Dialogs
{
    public class CheckAccountBalanceDialog : CancelAndHelpDialog
    {
        private readonly IAccountService _accountService;
        private readonly ICustomerService _customerService;

        public CheckAccountBalanceDialog(
            IAccountService accountService,
            ICustomerService customerService,
            AuthDialog authDialog
        )
     : base(nameof(CheckAccountBalanceDialog))
        {
            _accountService = accountService;
            _customerService = customerService;
            AddDialog(authDialog);
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
        AuthenticateUser,
        DisplayAccountBalance,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AuthenticateUser(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(AuthDialog), null, cancellationToken);
        }






        private async Task<DialogTurnResult> DisplayAccountBalance(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //var account = (Account)stepContext.Values["Account"];
            try
            {
                var balance = await _accountService.GetAccountBalanceAsync(1);

                if (balance != null)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Your account balance is: {balance.Currency} {balance.Balance:N2}"), cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Failed to retrieve account balance."), cancellationToken);
                }

                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            catch (Exception ex)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(ex.Message), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }






        }
    }
}






























/*private async Task<DialogTurnResult> InputAccountNumberAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
{
    var promptOptions = new PromptOptions
    {
        Prompt = MessageFactory.Text("Please enter your account number.")
    };

    return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
}

private async Task<DialogTurnResult> ProcessAccountNumberAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
{
    string accountNumber = stepContext.Result.ToString();



    if (!string.IsNullOrEmpty(accountNumber))
    {
        try
        {
            Account account = await _accountService.GetAccountByAccountNumber(accountNumber);

            stepContext.Values["account"] = account;


            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Hollup! I found your account!"), cancellationToken);

            return await VerifyOtp(stepContext, cancellationToken);

        }
        catch (Exception)
        {
            var response = "Sorry, I could not find an account with that account number!\nYou're welcome to create an account with  us.";

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(response), cancellationToken);

            return await stepContext.EndDialogAsync(null, cancellationToken);

        }
        finally
        {
            var account = (Account)stepContext.Values["account"];
            Customer customer = await _customerService.GetCustomerInfoAsync(account.Id);
            stepContext.Values["customer"] = customer;
        }

    }
    else
    {
        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please provide a valid account number."), cancellationToken);
    }

    return await stepContext.ReplaceDialogAsync(nameof(ProcessAccountNumberAsync), cancellationToken: cancellationToken);

}

private async Task<DialogTurnResult> VerifyOtp(WaterfallStepContext stepContext, CancellationToken cancellationToken)
{
    Random random = new Random();
    int otp = random.Next(1000, 9999);

    stepContext.Values["otp"] = otp;
    var customer = (Customer)stepContext.Values["customer"];

    var promptOptions = new PromptOptions
    {
        Prompt = MessageFactory.Text($"Enter the OTP sent to your mobile number {customer.PhoneNumber} and Email address {customer.Email}: {otp}"),
        RetryPrompt = MessageFactory.Text("Invalid input. Please enter a valid OTP.")
    };

    return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
}

private async Task<DialogTurnResult> ProcessOtp(WaterfallStepContext stepContext, CancellationToken cancellationToken)
{
    int otp = (int)stepContext.Values["otp"];
    int reply = int.Parse(stepContext.Result.ToString());

    if (reply == otp)
    {
        await stepContext.Context.SendActivityAsync(MessageFactory.Text("OTP verified."), cancellationToken);
        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Account Number Verified"), cancellationToken);
    }
    else
    {
        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Invalid OTP entered."), cancellationToken);
    }

    return await stepContext.NextAsync();
}*/