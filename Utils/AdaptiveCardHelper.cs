using System;
using System.IO;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace ChatBot.Utils
{
	public class AdaptiveCardHelper
	{

        public static Attachment CreateAdaptiveCardAttachment(string cardName)
        {
            var filePath = Path.Combine(".", "Resources", $"{cardName}.json");
            var adaptiveCardJson = File.ReadAllText(filePath);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }
    }
}

