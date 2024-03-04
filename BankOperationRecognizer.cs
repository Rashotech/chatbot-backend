using System;
using ChatBot.Clu;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using DotNetEnv;

namespace ChatBot
{
	public class BankOperationRecognizer : IRecognizer
    {
        private readonly CluRecognizer _recognizer;

        public BankOperationRecognizer()
        {
            var cluIsConfigured = !string.IsNullOrEmpty(Env.GetString("CluProjectName")) && !string.IsNullOrEmpty(Env.GetString("CluDeploymentName")) && !string.IsNullOrEmpty(Env.GetString("CluAPIKey")) && !string.IsNullOrEmpty(Env.GetString("CluAPIHostName"));
            if (cluIsConfigured)
            {
                var cluApplication = new CluApplication(
                    Env.GetString("CluProjectName"),
                    Env.GetString("CluDeploymentName"),
                    Env.GetString("CluAPIKey"),
                    "https://" + Env.GetString("CluAPIHostName"));
                var recognizerOptions = new CluOptions(cluApplication) { Language = "en" };

                _recognizer = new CluRecognizer(recognizerOptions);
            }
        }

        // Returns true if clu is configured in the appsettings.json and initialized.
        public virtual bool IsConfigured => _recognizer != null;

        public virtual async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
            => await _recognizer.RecognizeAsync(turnContext, cancellationToken);

        public virtual async Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken)
            where T : IRecognizerConvert, new()
            => await _recognizer.RecognizeAsync<T>(turnContext, cancellationToken);
    }
}

