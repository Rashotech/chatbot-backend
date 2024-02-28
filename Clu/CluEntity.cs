using System;
using Newtonsoft.Json;

namespace ChatBot.Clu
{
    public class CluEntity
    {
        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("offset")]
        public int Offset { get; set; }

        [JsonProperty("length")]
        public int Length { get; set; }

        [JsonProperty("confidenceScore")]
        public float ConfidenceScore { get; set; }
    }
}

