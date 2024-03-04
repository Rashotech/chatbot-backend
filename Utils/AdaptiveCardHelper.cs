using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        public static Attachment CreateAdaptiveCardAttachment(string cardName, Dictionary<string, object> variables)
        {
            var filePath = Path.Combine(".", "Resources", $"{cardName}.json");
            var adaptiveCardJson = File.ReadAllText(filePath);

            // Replace variables in the JSON
            foreach (var variable in variables)
            {
                adaptiveCardJson = adaptiveCardJson.Replace($"{{{{{variable.Key}}}}}", variable.Value.ToString());
            }

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }

        public static Attachment CreateAdaptiveCardAttachment<T>(string cardName, List<T> dataList, Func<T, string> getTitle, Func<T, string> getValue)
        {
            var filePath = Path.Combine(".", "Resources", $"{cardName}.json");
            var adaptiveCardJson = File.ReadAllText(filePath);

            // Create an array to hold the choices
            var choicesArray = new JArray();

            // Populate the choices array dynamically
            foreach (var dataItem in dataList)
            {
                var choice = new JObject
                {
                    ["title"] = getTitle(dataItem), // Get the title using the specified function
                    ["value"] = getValue(dataItem) // Get the value using the specified function
                };
                choicesArray.Add(choice);
            }

            // Replace the placeholder in the JSON with the dynamically generated choices array
            var cardJson = JObject.Parse(adaptiveCardJson);
            var choicesInput = cardJson.SelectToken("$.body[0].choices");
            choicesInput.Replace(choicesArray);

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = cardJson,
            };

            return adaptiveCardAttachment;
        }

        public static Attachment CreateTableAdaptiveCardAttachment<T>(string cardName, IEnumerable<T> dataList, Func<T, string> getNarration, Func<T, string> getDate, Func<T, string> getAmount)
        {
            var filePath = Path.Combine(".", "Resources", $"{cardName}.json");
            var adaptiveCardJson = File.ReadAllText(filePath);

            var cardJson = JObject.Parse(adaptiveCardJson);
            var container = (JArray)cardJson.SelectToken("$.body[1].items");

            foreach (var dataItem in dataList)
            {
                var narration = getNarration(dataItem);
                var date = getDate(dataItem);
                var amount = getAmount(dataItem);

                var columnSet = new JObject
                {
                    ["type"] = "ColumnSet",
                    ["separator"] = true,
                    ["columns"] = new JArray
            {
                new JObject
                {
                    ["type"] = "Column",
                    ["width"] = "stretch",
                    ["items"] = new JArray
                    {
                        new JObject
                        {
                            ["type"] = "TextBlock",
                            ["text"] = narration,
                            ["wrap"] = true
                        },
                        new JObject
                        {
                            ["type"] = "TextBlock",
                            ["text"] = date
                        }
                    }
                },
                new JObject
                {
                    ["type"] = "Column",
                    ["width"] = "auto",
                    ["items"] = new JArray
                    {
                        new JObject
                        {
                            ["type"] = "TextBlock",
                            ["text"] = amount
                        }
                    }
                }
            }
                };

                container.Add(columnSet);
            }

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = cardJson
            };

            return adaptiveCardAttachment;
        }

    }
}

