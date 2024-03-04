using ChatBot.Database.Models;
using ChatBot.Services.Interfaces;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBot.Dialogs
{
    public class FeedbackDialog : CancelAndHelpDialog
    {
        private readonly IStatePropertyAccessor<Account> _accountInfoAccessor;
        private readonly IAccountService _accountService; 

        public FeedbackDialog(
            IAccountService accountService,
            UserState userState
        )
     : base(nameof(FeedbackDialog))
        {
            _accountService = accountService;
            _accountInfoAccessor = userState.CreateProperty<Account>("Account");
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                RatingStepAsync,
                AcknowledgeRatingStepAsync,
                ReviewStepAsync,
                FinalStepAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> RatingStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
                var reply = MessageFactory.Text(
                 "**Rate your experience!**" +
                 "\n\nPlease rate your experience! Your feedback is very appreciated and will help improve your experience in the future."
                 );

                reply.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                    {
                        new CardAction() { Title = "Very Poor", Type = ActionTypes.ImBack, Value = "Very Poor", Image = "https://upload.wikimedia.org/wikipedia/commons/thumb/a/ae/Noto_Emoji_R_1f97a.svg/32px-Noto_Emoji_R_1f97a.svg.png"},
                        new CardAction() { Title = "Poor", Type = ActionTypes.ImBack, Value = "Poor", Image = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e3/MOREmoji_pout.svg/32px-MOREmoji_pout.svg.png" },
                        new CardAction() { Title = "Average", Type = ActionTypes.ImBack, Value = "Average", Image = "https://upload.wikimedia.org/wikipedia/commons/thumb/4/4d/Emojione_1F610.svg/32px-Emojione_1F610.svg.png" },
                        new CardAction() { Title = "Good", Type = ActionTypes.ImBack, Value = "Good", Image = "https://upload.wikimedia.org/wikipedia/commons/thumb/7/72/Twemoji2_1f642.svg/32px-Twemoji2_1f642.svg.png" },
                        new CardAction() { Title = "Excellent", Type = ActionTypes.ImBack, Value = "Excellent", Image = "https://upload.wikimedia.org/wikipedia/commons/thumb/a/a3/Noto_Emoji_Pie_1f600.svg/32px-Noto_Emoji_Pie_1f600.svg.png" }
                    },
                };

                await stepContext.Context.SendActivityAsync(reply, cancellationToken);

                return Dialog.EndOfTurn;
        }

        private async Task<DialogTurnResult> AcknowledgeRatingStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var rating = (string)stepContext.Result;
            stepContext.Values["rating"] = rating;
            var messageText = $"You have just selected rating - {rating}";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(messageText), cancellationToken);
            return await stepContext.NextAsync(rating, cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> ReviewStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptText = $"Please provide additional comments so we can improve";
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text(promptText),
            };

            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);           
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //var review = (string)stepContext.Result;
            //var rating = (string)stepContext.Values["rating"];

            var messageText = $"We appreciate your feedback!";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(messageText), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}




