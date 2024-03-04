using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;

namespace ChatBot.CognitiveModels
{
    public class BankOperation : IRecognizerConvert
    {
        public enum Intent
        {
            AccountOpening,
            BranchLocator,
            LogComplaints,
            TrackComplaintStatus,
            ManageComplaint,
            Cancel,
            CheckingBalance,
            FundTransfer,
            GetTransactionHistory,
            None
        }

        public string Text { get; set; }

        public string AlteredText { get; set; }

        public Dictionary<Intent, IntentScore> Intents { get; set; }

        public IDictionary<string, object> Properties { get; set; }

        public void Convert(dynamic result)
        {
            var jsonResult = JsonConvert.SerializeObject(result, new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
            var app = JsonConvert.DeserializeObject<BankOperation>(jsonResult);

            Text = app.Text;
            AlteredText = app.AlteredText;
            Intents = app.Intents;
            Properties = app.Properties;
        }

        public (Intent intent, double score) GetTopIntent()
        {
            var maxIntent = Intent.None;
            var max = 0.0;
            foreach (var entry in Intents)
            {
                if (entry.Value.Score > max)
                {
                    maxIntent = entry.Key;
                    max = entry.Value.Score.Value;
                }
            }

            return (maxIntent, max);
        }
    }
}
