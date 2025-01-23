using System.Text.Json.Serialization;
using System.Text.Json;
using Newtonsoft.Json;

namespace AISoccerAPI.API.SoccerAPI.SoccerSeasonDetail
{

    #region Get Season Details

    public class GetSeasonDetails
    {
        public async Task<SeasonDetailsResponse> GetSeasonDetailsAsync(string user, string token, string seasonId)
        {
            try
            {
                HttpClient client = new HttpClient();
                string url = $"https://api.soccersapi.com/v2.2/seasons/?user={user}&token={token}&t=info&id={seasonId}";
                HttpResponseMessage response = await client.GetAsync(url);
                var apiSeasonDetailsResponses = JsonConvert.DeserializeObject<SeasonDetailsResponse>(await response.Content.ReadAsStringAsync());
                return apiSeasonDetailsResponses;
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

    public class SeasonDetailsResponse
    {
        [JsonPropertyName("data")]
        [JsonProperty("data")] // Uncomment for Newtonsoft.Json
        public SeasonDetails Data { get; set; }

        [JsonPropertyName("meta")]
        [JsonProperty("meta")] // Uncomment for Newtonsoft.Json
        public Meta Meta { get; set; }
    }

    public class SeasonDetails
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

        [JsonPropertyName("start")]
        [JsonProperty("start")] // Uncomment for Newtonsoft.Json
        public DateTime Start { get; set; }

        [JsonPropertyName("end")]
        [JsonProperty("end")] // Uncomment for Newtonsoft.Json
        public DateTime End { get; set; }

        [JsonPropertyName("league_id")]
        [JsonProperty("league_id")] // Uncomment for Newtonsoft.Json
        public string LeagueId { get; set; }

        [JsonPropertyName("current_round_id")]
        [JsonProperty("current_round_id")] // Uncomment for Newtonsoft.Json
        public string CurrentRoundId { get; set; }

        [JsonPropertyName("current_stage_id")]
        [JsonProperty("current_stage_id")] // Uncomment for Newtonsoft.Json
        public string CurrentStageId { get; set; }

        [JsonPropertyName("round_ids")]
        [JsonProperty("round_ids")] // Uncomment for Newtonsoft.Json
        public List<string> RoundIds { get; set; }

        [JsonPropertyName("stages")]
        [JsonProperty("stages")] // Uncomment for Newtonsoft.Json
        public List<Stage> Stages { get; set; }
    }

    public class Stage
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")] // Uncomment for Newtonsoft.Json
        public int Id { get; set; }

        [JsonPropertyName("name")]
        [JsonProperty("name")] // Uncomment for Newtonsoft.Json
        public string Name { get; set; }

        [JsonPropertyName("is_current")]
        [JsonProperty("is_current")] // Uncomment for Newtonsoft.Json
        public int IsCurrent { get; set; }
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

        [JsonPropertyName("total")]
        [JsonProperty("total")] // Uncomment for Newtonsoft.Json
        public int Total { get; set; }

        [JsonPropertyName("msg")]
        [JsonProperty("msg")] // Uncomment for Newtonsoft.Json
        public string Msg { get; set; }
    }

    #endregion
}
