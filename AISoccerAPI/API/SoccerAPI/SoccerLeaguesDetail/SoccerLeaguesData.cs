using System.Text.Json.Serialization;
using System.Text.Json;
using Newtonsoft.Json;

namespace AISoccerAPI.API.SoccerAPI.SoccerLeaguesDetail
{

    #region Get Leagues

    public class GetLeagues
    {
        public async Task<SoccerApiLeagueResponse> GetLeaguesAsync(string user, string token)
        {
            try
            {
                HttpClient client = new HttpClient();
                string url = $"https://api.soccersapi.com/v2.2/leagues/?user={user}&token={token}&t=list";
                HttpResponseMessage response = await client.GetAsync(url);
                var apiLeagueResponses = JsonConvert.DeserializeObject<SoccerApiLeagueResponse>(await response.Content.ReadAsStringAsync());
                return apiLeagueResponses;
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP request error: {httpEx.Message}");
                throw; // Re-throws the exception to propagate it if necessary
            }
            catch (Newtonsoft.Json.JsonException jsonEx)
            {
                Console.WriteLine($"JSON deserialization error: {jsonEx.Message}");
                throw; // Re-throws the exception to propagate it if necessary
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                throw; // Re-throws the exception to propagate it if necessary
            }            
        }
    }

    public class SoccerApiLeagueResponse
    {
        [JsonPropertyName("data")]
        [JsonProperty("data")] // Uncomment for Newtonsoft.Json
        public List<League> Data { get; set; }

        [JsonPropertyName("meta")]
        [JsonProperty("meta")] // Uncomment for Newtonsoft.Json
        public Meta Meta { get; set; }
    }

    public class League
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")] // Uncomment for Newtonsoft.Json
        public string Id { get; set; }

        [JsonPropertyName("name")]
        [JsonProperty("name")] // Uncomment for Newtonsoft.Json
        public string Name { get; set; }

        [JsonPropertyName("is_cup")]
        [JsonProperty("is_cup")] // Uncomment for Newtonsoft.Json
        public string IsCup { get; set; }

        [JsonPropertyName("is_amateur")]
        [JsonProperty("is_amateur")] // Uncomment for Newtonsoft.Json
        public string IsAmateur { get; set; }

        [JsonPropertyName("is_friendly")]
        [JsonProperty("is_friendly")] // Uncomment for Newtonsoft.Json
        public string IsFriendly { get; set; }

        [JsonPropertyName("continent_id")]
        [JsonProperty("continent_id")] // Uncomment for Newtonsoft.Json
        public string ContinentId { get; set; }

        [JsonPropertyName("continent_name")]
        [JsonProperty("continent_name")] // Uncomment for Newtonsoft.Json
        public string ContinentName { get; set; }

        [JsonPropertyName("country_id")]
        [JsonProperty("country_id")] // Uncomment for Newtonsoft.Json
        public string CountryId { get; set; }

        [JsonPropertyName("country_name")]
        [JsonProperty("country_name")] // Uncomment for Newtonsoft.Json
        public string CountryName { get; set; }

        [JsonPropertyName("cc")]
        [JsonProperty("cc")] // Uncomment for Newtonsoft.Json
        public string CountryCode { get; set; }

        [JsonPropertyName("current_season_id")]
        [JsonProperty("current_season_id")] // Uncomment for Newtonsoft.Json
        public string CurrentSeasonId { get; set; }

        [JsonPropertyName("current_round_id")]
        [JsonProperty("current_round_id")] // Uncomment for Newtonsoft.Json
        public string CurrentRoundId { get; set; }

        [JsonPropertyName("current_stage_id")]
        [JsonProperty("current_stage_id")] // Uncomment for Newtonsoft.Json
        public string CurrentStageId { get; set; }
    }

    public class Meta
    {
        [JsonPropertyName("requests_left")]
        [JsonProperty("requests_left")] // Uncomment for Newtonsoft.Json
        public int RequestsLeft { get; set; }

        [JsonPropertyName("user")]
        [JsonProperty("user")] // Uncomment for Newtonsoft.Json
        public string User { get; set; }

        [JsonPropertyName("plan")]
        [JsonProperty("plan")] // Uncomment for Newtonsoft.Json
        public string Plan { get; set; }

        [JsonPropertyName("pages")]
        [JsonProperty("pages")] // Uncomment for Newtonsoft.Json
        public int Pages { get; set; }

        [JsonPropertyName("page")]
        [JsonProperty("page")] // Uncomment for Newtonsoft.Json
        public int Page { get; set; }

        [JsonPropertyName("count")]
        [JsonProperty("count")] // Uncomment for Newtonsoft.Json
        public int Count { get; set; }

        [JsonPropertyName("total")]
        [JsonProperty("total")] // Uncomment for Newtonsoft.Json
        public int Total { get; set; }

        [JsonPropertyName("msg")]
        [JsonProperty("msg")] // Uncomment for Newtonsoft.Json
        public string Msg { get; set; }
    }

    #endregion

}
