using System;
using System.Threading;
using System.Threading.Tasks;
using ChatBot.Dialogs;
using DotNetEnv;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA.Dialogs;
using Microsoft.Bot.Builder.AI.QnA.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;

namespace ChatBot.Dialogs
{
    public class QnADialog : CancelAndHelpDialog
    {
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
            : base("root")
        {
            AddDialog(CreateQnAMakerDialog(configuration));

            AddDialog(new WaterfallDialog(DialogId)
               .AddStep(InitialStepAsync));

            // The initial child Dialog to run.
            InitialDialogId = DialogId;
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
            var useTeamsAdaptiveCard = bool.Parse(configuration["UseTeamsAdaptiveCard"]);

            // Create a new instance of QnAMakerDialog with dialogOptions initialized.
            var noAnswer = MessageFactory.Text(configuration["DefaultAnswer"] ?? string.Empty);
            var qnamakerDialog = new QnAMakerDialog(nameof(QnAMakerDialog), knowledgeBaseId, endpointKey, hostname, noAnswer: noAnswer, cardNoMatchResponse: MessageFactory.Text(ActiveLearningCardNoMatchResponse), useTeamsAdaptiveCard: useTeamsAdaptiveCard)
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
                UseTeamsAdaptiveCard = useTeamsAdaptiveCard
            };

            return qnamakerDialog;
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(QnAMakerDialog), null, cancellationToken);
        }
    }
}

