using System.Text.Json.Serialization;
using System.Text.Json;
using Newtonsoft.Json;
using AISoccerAPI.API.SoccerAPI.SoccerLeaguesDetail;

namespace AISoccerAPI.API.SoccerAPI.SoccerLeagueDetail
{
    #region Get League Details

    public class GetLeagueDetail
    {
        public async Task<LeagueDetailsResponse> GetSoccerLeagueAsync(string user, string token, string leagueId)
        {
            try
            {
                HttpClient client = new HttpClient();
                string url = $"https://api.soccersapi.com/v2.2/leagues/?user={user}&token={token}&t=info&id={leagueId}";
                HttpResponseMessage response = await client.GetAsync(url);
                var apiLeagueDetailResponses = JsonConvert.DeserializeObject<LeagueDetailsResponse>(await response.Content.ReadAsStringAsync());
                return apiLeagueDetailResponses;
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


    public class LeagueDetailsResponse
    {
        [JsonPropertyName("data")]
        [JsonProperty("data")] // Uncomment for Newtonsoft.Json
        public LeagueDetails Data { get; set; }

        [JsonPropertyName("meta")]
        [JsonProperty("meta")] // Uncomment for Newtonsoft.Json
        public Meta Meta { get; set; }
    }

    public class LeagueDetails
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")] // Uncomment for Newtonsoft.Json
        public int Id { get; set; }

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

        [JsonPropertyName("img")]
        [JsonProperty("img")] // Uncomment for Newtonsoft.Json
        public string ImageUrl { get; set; }

        [JsonPropertyName("id_current_season")]
        [JsonProperty("id_current_season")] // Uncomment for Newtonsoft.Json
        public string CurrentSeasonId { get; set; }

        [JsonPropertyName("id_current_stage")]
        [JsonProperty("id_current_stage")] // Uncomment for Newtonsoft.Json
        public string CurrentStageId { get; set; }

        [JsonPropertyName("id_current_round")]
        [JsonProperty("id_current_round")] // Uncomment for Newtonsoft.Json
        public string CurrentRoundId { get; set; }

        [JsonPropertyName("default_color")]
        [JsonProperty("default_color")] // Uncomment for Newtonsoft.Json
        public string DefaultColor { get; set; }

        [JsonPropertyName("continent")]
        [JsonProperty("continent")] // Uncomment for Newtonsoft.Json
        public Continent Continent { get; set; }

        [JsonPropertyName("country")]
        [JsonProperty("country")] // Uncomment for Newtonsoft.Json
        public Country Country { get; set; }

        [JsonPropertyName("seasons")]
        [JsonProperty("seasons")] // Uncomment for Newtonsoft.Json
        public List<Season> Seasons { get; set; }
    }

    public class Continent
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")] // Uncomment for Newtonsoft.Json
        public int Id { get; set; }

        [JsonPropertyName("name")]
        [JsonProperty("name")] // Uncomment for Newtonsoft.Json
        public string Name { get; set; }
    }

    public class Country
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")] // Uncomment for Newtonsoft.Json
        public int Id { get; set; }

        [JsonPropertyName("name")]
        [JsonProperty("name")] // Uncomment for Newtonsoft.Json
        public string Name { get; set; }

        [JsonPropertyName("cc")]
        [JsonProperty("cc")] // Uncomment for Newtonsoft.Json
        public string CountryCode { get; set; }
    }

    public class Season
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")] // Uncomment for Newtonsoft.Json
        public string Id { get; set; }

        [JsonPropertyName("name")]
        [JsonProperty("name")] // Uncomment for Newtonsoft.Json
        public string Name { get; set; }

        [JsonPropertyName("is_current")]
        [JsonProperty("is_current")] // Uncomment for Newtonsoft.Json
        public string IsCurrent { get; set; }
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
