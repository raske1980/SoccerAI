using System;
using Newtonsoft.Json;

namespace AISoccerAPI.JSON.OpenData.Data
{
    public class Competition
    {
        [JsonProperty("competition_id")]
        public int CompetitionId { get; set; }

        [JsonProperty("season_id")]
        public int SeasonId { get; set; }

        [JsonProperty("country_name")]
        public string CountryName { get; set; }

        [JsonProperty("competition_name")]
        public string CompetitionName { get; set; }

        [JsonProperty("competition_gender")]
        public string CompetitionGender { get; set; }

        [JsonProperty("competition_youth")]
        public bool CompetitionYouth { get; set; }

        [JsonProperty("competition_international")]
        public bool CompetitionInternational { get; set; }

        [JsonProperty("season_name")]
        public string SeasonName { get; set; }

        [JsonProperty("match_updated")]
        public DateTime? MatchUpdated { get; set; }

        [JsonProperty("match_updated_360")]
        public DateTime? MatchUpdated360 { get; set; }

        [JsonProperty("match_available_360")]
        public DateTime? MatchAvailable360 { get; set; }

        [JsonProperty("match_available")]
        public DateTime? MatchAvailable { get; set; }
    }
}

