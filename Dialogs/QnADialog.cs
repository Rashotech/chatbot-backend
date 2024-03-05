using System;
using System.Threading;
using System.Threading.Tasks;
using ChatBot.Models;
using DotNetEnv;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA.Dialogs;
using Microsoft.Bot.Builder.AI.QnA.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace ChatBot.Dialogs
{
    public class QnADialog : CancelAndHelpDialog
    {
        private readonly string ConfirmDlgId = "ConfirmDlgId";
        private readonly string Confirm2DlgId = "Confirm2DlgId";

        private const string DialogId = "initial-dialog";
        private const string ActiveLearningCardTitle = "Did you mean:";
        private const string ActiveLearningCardNoMatchText = "None of the above.";
        private const string ActiveLearningCardNoMatchResponse = "Thanks for the feedback.";

        private const float ScoreThreshold = 0.3f;
        private const int TopAnswers = 3;
        private const string RankerType = "Default";
        private const bool IsTest = false;
        private const bool IncludeUnstructuredSources = true;

        public QnADialog(IConfiguration configuration)
            : base(nameof(QnADialog))
        {
            AddDialog(CreateQnAMakerDialog(configuration));
            AddDialog(new ConfirmPrompt(ConfirmDlgId));
            AddDialog(new ConfirmPrompt(Confirm2DlgId));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                InitialStepAsync,
                AskForFeedbackStepAsync,
                AskforOtherQuestiionStepAsync,
                FinalStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private QnAMakerDialog CreateQnAMakerDialog(IConfiguration configuration)
        {
            const string missingConfigError = "{0} is missing or empty in configuration.";

            var hostname = configuration["LanguageEndpointHostName"];
            if (string.IsNullOrEmpty(hostname))
            {
                throw new ArgumentException(string.Format(missingConfigError, "LanguageEndpointHostName"));
            }

            var endpointKey = Env.GetString("CluAPIKey");
            if (string.IsNullOrEmpty(endpointKey))
            {
                throw new ArgumentException(string.Format(missingConfigError, "LanguageEndpointKey"));
            }

            var knowledgeBaseId = configuration["ProjectName"];
            if (string.IsNullOrEmpty(knowledgeBaseId))
            {
                throw new ArgumentException(string.Format(missingConfigError, "ProjectName"));
            }

            var enablePreciseAnswer = bool.Parse(configuration["EnablePreciseAnswer"]);
            var displayPreciseAnswerOnly = bool.Parse(configuration["DisplayPreciseAnswerOnly"]);

            // Create a new instance of QnAMakerDialog with dialogOptions initialized.
            var noAnswer = MessageFactory.Text(configuration["DefaultAnswer"] ?? string.Empty);
            var qnamakerDialog = new QnAMakerDialog(nameof(QnAMakerDialog), knowledgeBaseId, endpointKey, hostname, noAnswer: noAnswer, cardNoMatchResponse: MessageFactory.Text(ActiveLearningCardNoMatchResponse), useTeamsAdaptiveCard: false)
            {
                Threshold = ScoreThreshold,
                ActiveLearningCardTitle = ActiveLearningCardTitle,
                CardNoMatchText = ActiveLearningCardNoMatchText,
                Top = TopAnswers,
                Filters = { },
                QnAServiceType = ServiceType.Language,
                EnablePreciseAnswer = enablePreciseAnswer,
                DisplayPreciseAnswerOnly = displayPreciseAnswerOnly,
                IncludeUnstructuredSources = IncludeUnstructuredSources,
                RankerType = RankerType,
                IsTest = IsTest,
                UseTeamsAdaptiveCard = false
            };

            return qnamakerDialog;
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var questionAnswering = (QuestionAnswering)stepContext.Options;
            bool ask = questionAnswering == null || questionAnswering.Skip == false;

            if (ask)
            {
                var promptMessage = MessageFactory.Text("Feel free to ask me any question related to FirstBank Product and Services");
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(QnAMakerDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> AskForFeedbackStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var messageText = "Are you satisfied with the response?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(ConfirmDlgId, new PromptOptions
            {
                Prompt = promptMessage
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> AskforOtherQuestiionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result != false)
            {
                var messageText = "Do you have another question for me?";
                var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(Confirm2DlgId, new PromptOptions
                {
                    Prompt = promptMessage
                }, cancellationToken);
            }
            else
            {
                return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result != false)
            {
                return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

    }
}