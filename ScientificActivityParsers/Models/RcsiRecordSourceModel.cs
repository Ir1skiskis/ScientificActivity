using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityParsers.Models
{
    public class RcsiRecordSourceModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("url")]
        public string? Url { get; set; }

        [JsonProperty("title")]
        public List<string> Title { get; set; } = new();

        [JsonProperty("issn")]
        public List<string> Issn { get; set; } = new();

        [JsonProperty("issns")]
        private List<string> IssnsAlt
        {
            set
            {
                if (value != null && value.Count > 0 && Issn.Count == 0)
                {
                    Issn = value;
                }
            }
        }

        [JsonProperty("level_2023")]
        public int? Level2023 { get; set; }

        [JsonProperty("level_2025")]
        public int? Level2025 { get; set; }

        [JsonProperty("dateAccepted")]
        public string? DateAccepted { get; set; }

        [JsonProperty("date_accepted")]
        private string? DateAcceptedAlt
        {
            set
            {
                if (string.IsNullOrWhiteSpace(DateAccepted))
                {
                    DateAccepted = value;
                }
            }
        }

        [JsonProperty("dateDiscontinued")]
        public string? DateDiscontinued { get; set; }

        [JsonProperty("date_discontinued")]
        private string? DateDiscontinuedAlt
        {
            set
            {
                if (string.IsNullOrWhiteSpace(DateDiscontinued))
                {
                    DateDiscontinued = value;
                }
            }
        }

        [JsonProperty("state")]
        public string? State { get; set; }

        [JsonProperty("notice")]
        public string? Notice { get; set; }

        [JsonIgnore]
        public List<string> Issns => Issn;
    }
}
