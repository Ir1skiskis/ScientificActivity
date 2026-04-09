using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityParsers.Models
{
    public class RcsiLevelApiResponseModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("url")]
        public string? Url { get; set; }

        [JsonProperty("title")]
        public List<string> Title { get; set; } = new();

        [JsonProperty("issn")]
        public List<string> Issn { get; set; } = new();

        [JsonProperty("level_2023")]
        public int? Level2023 { get; set; }

        [JsonProperty("level_2025")]
        public int? Level2025 { get; set; }

        [JsonProperty("state")]
        public string? State { get; set; }

        [JsonProperty("notice")]
        public string? Notice { get; set; }

        [JsonProperty("dateAccepted")]
        public string? DateAccepted { get; set; }

        [JsonProperty("dateDiscontinued")]
        public string? DateDiscontinued { get; set; }
    }
}
