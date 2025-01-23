using System.Text.Json.Serialization;
using System.Text.Json;
using Newtonsoft.Json;
using AISoccerAPI.API.SoccerAPI.SoccerLeaguesDetail;
using AISoccerAPI.API.SoccerAPI.SoccerSeasonDetail;

namespace AISoccerAPI.API.SoccerAPI.SoccerSeasonMathesDetail
{

    public class GetSeasonMatchDetails
    {
        public async Task<SeasonMatchesDetails> GetSeasonMatchDetailsAsync(string user, string token, string seasonId)
        {
            try
            {
                HttpClient client = new HttpClient();
                string url = $"https://api.soccersapi.com/v2.2/fixtures/?user={user}&token={token}&t=season&season_id={seasonId}";
                HttpResponseMessage response = await client.GetAsync(url);
                string responseString = await response.Content.ReadAsStringAsync();
                var apiSeasonDetailsResponses = JsonConvert.DeserializeObject<SeasonMatchesDetails>(responseString);
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

    public class Assistants
    {
        [JsonPropertyName("first_assistant_id")]
        [JsonProperty("first_assistant_id")]
        public object FirstAssistantId { get; set; }

        [JsonPropertyName("second_assistant_id")]
        [JsonProperty("second_assistant_id")]
        public object SecondAssistantId { get; set; }

        [JsonPropertyName("fourth_assistant_id")]
        [JsonProperty("fourth_assistant_id")]
        public object FourthAssistantId { get; set; }
    }

    public class Away
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonPropertyName("short_code")]
        [JsonProperty("short_code")]
        public string ShortCode { get; set; }

        [JsonPropertyName("img")]
        [JsonProperty("img")]
        public string Img { get; set; }

        [JsonPropertyName("form")]
        [JsonProperty("form")]
        public string Form { get; set; }

        [JsonPropertyName("coach_id")]
        [JsonProperty("coach_id")]
        public string CoachId { get; set; }

        [JsonPropertyName("kit_colors")]
        [JsonProperty("kit_colors")]
        public KitColors KitColors { get; set; }
    }

    public class Coverage
    {
        [JsonPropertyName("has_lineups")]
        [JsonProperty("has_lineups")]
        public int HasLineups { get; set; }

        [JsonPropertyName("has_tvs")]
        [JsonProperty("has_tvs")]
        public int HasTvs { get; set; }

        [JsonPropertyName("has_standings")]
        [JsonProperty("has_standings")]
        public int HasStandings { get; set; }
    }

    public class Datum
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonPropertyName("status")]
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonPropertyName("status_name")]
        [JsonProperty("status_name")]
        public string StatusName { get; set; }

        [JsonPropertyName("status_period")]
        [JsonProperty("status_period")]
        public object StatusPeriod { get; set; }

        [JsonPropertyName("pitch")]
        [JsonProperty("pitch")]
        public object Pitch { get; set; }

        [JsonPropertyName("referee_id")]
        [JsonProperty("referee_id")]
        public string RefereeId { get; set; }

        [JsonPropertyName("round_id")]
        [JsonProperty("round_id")]
        public string RoundId { get; set; }

        [JsonPropertyName("round_name")]
        [JsonProperty("round_name")]
        public string RoundName { get; set; }

        [JsonPropertyName("season_id")]
        [JsonProperty("season_id")]
        public string SeasonId { get; set; }

        [JsonPropertyName("season_name")]
        [JsonProperty("season_name")]
        public string SeasonName { get; set; }

        [JsonPropertyName("stage_id")]
        [JsonProperty("stage_id")]
        public string StageId { get; set; }

        [JsonPropertyName("stage_name")]
        [JsonProperty("stage_name")]
        public string StageName { get; set; }

        [JsonPropertyName("group_id")]
        [JsonProperty("group_id")]
        public string GroupId { get; set; }

        [JsonPropertyName("group_name")]
        [JsonProperty("group_name")]
        public string GroupName { get; set; }

        [JsonPropertyName("aggregate_id")]
        [JsonProperty("aggregate_id")]
        public object AggregateId { get; set; }

        [JsonPropertyName("winner_team_id")]
        [JsonProperty("winner_team_id")]
        public string WinnerTeamId { get; set; }

        [JsonPropertyName("venue_id")]
        [JsonProperty("venue_id")]
        public string VenueId { get; set; }

        [JsonPropertyName("leg")]
        [JsonProperty("leg")]
        public object Leg { get; set; }

        [JsonPropertyName("week")]
        [JsonProperty("week")]
        public string Week { get; set; }

        [JsonPropertyName("deleted")]
        [JsonProperty("deleted")]
        public string Deleted { get; set; }

        [JsonPropertyName("related_id")]
        [JsonProperty("related_id")]
        public object RelatedId { get; set; }

        [JsonPropertyName("info")]
        [JsonProperty("info")]
        public object Info { get; set; }

        [JsonPropertyName("attendance")]
        [JsonProperty("attendance")]
        public object Attendance { get; set; }

        [JsonPropertyName("time")]
        [JsonProperty("time")]
        public Time Time { get; set; }

        [JsonPropertyName("teams")]
        [JsonProperty("teams")]
        public Teams Teams { get; set; }

        [JsonPropertyName("league")]
        [JsonProperty("league")]
        public League League { get; set; }

        [JsonPropertyName("scores")]
        [JsonProperty("scores")]
        public Scores Scores { get; set; }

        [JsonPropertyName("standings")]
        [JsonProperty("standings")]
        public Standings Standings { get; set; }

        [JsonPropertyName("assistants")]
        [JsonProperty("assistants")]
        public Assistants Assistants { get; set; }

        [JsonPropertyName("coverage")]
        [JsonProperty("coverage")]
        public Coverage Coverage { get; set; }

        [JsonPropertyName("weather_report")]
        [JsonProperty("weather_report")]
        public object WeatherReport { get; set; }
    }

    public class Home
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonPropertyName("short_code")]
        [JsonProperty("short_code")]
        public string ShortCode { get; set; }

        [JsonPropertyName("img")]
        [JsonProperty("img")]
        public string Img { get; set; }

        [JsonPropertyName("form")]
        [JsonProperty("form")]
        public string Form { get; set; }

        [JsonPropertyName("coach_id")]
        [JsonProperty("coach_id")]
        public string CoachId { get; set; }

        [JsonPropertyName("kit_colors")]
        [JsonProperty("kit_colors")]
        public KitColors KitColors { get; set; }
    }

    public class KitColors
    {
        [JsonPropertyName("home_main_color")]
        [JsonProperty("home_main_color")]
        public string HomeMainColor { get; set; }

        [JsonPropertyName("home_second_color")]
        [JsonProperty("home_second_color")]
        public string HomeSecondColor { get; set; }

        [JsonPropertyName("home_number_color")]
        [JsonProperty("home_number_color")]
        public string HomeNumberColor { get; set; }

        [JsonPropertyName("home_gk_main_color")]
        [JsonProperty("home_gk_main_color")]
        public string HomeGkMainColor { get; set; }

        [JsonPropertyName("home_gk_second_color")]
        [JsonProperty("home_gk_second_color")]
        public string HomeGkSecondColor { get; set; }

        [JsonPropertyName("home_gk_number_color")]
        [JsonProperty("home_gk_number_color")]
        public string HomeGkNumberColor { get; set; }

        [JsonPropertyName("away_main_color")]
        [JsonProperty("away_main_color")]
        public string AwayMainColor { get; set; }

        [JsonPropertyName("away_second_color")]
        [JsonProperty("away_second_color")]
        public string AwaySecondColor { get; set; }

        [JsonPropertyName("away_number_color")]
        [JsonProperty("away_number_color")]
        public string AwayNumberColor { get; set; }

        [JsonPropertyName("away_gk_main_color")]
        [JsonProperty("away_gk_main_color")]
        public string AwayGkMainColor { get; set; }

        [JsonPropertyName("away_gk_second_color")]
        [JsonProperty("away_gk_second_color")]
        public string AwayGkSecondColor { get; set; }

        [JsonPropertyName("away_gk_number_color")]
        [JsonProperty("away_gk_number_color")]
        public string AwayGkNumberColor { get; set; }
    }

    public class League
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonPropertyName("country_id")]
        [JsonProperty("country_id")]
        public string CountryId { get; set; }

        [JsonPropertyName("country_name")]
        [JsonProperty("country_name")]
        public string CountryName { get; set; }

        [JsonPropertyName("country_code")]
        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        [JsonPropertyName("country_flag")]
        [JsonProperty("country_flag")]
        public string CountryFlag { get; set; }
    }

    public class Meta
    {
        [JsonPropertyName("requests_left")]
        [JsonProperty("requests_left")]
        public int RequestsLeft { get; set; }

        [JsonPropertyName("user")]
        [JsonProperty("user")]
        public string User { get; set; }

        [JsonPropertyName("plan")]
        [JsonProperty("plan")]
        public string Plan { get; set; }

        [JsonPropertyName("pages")]
        [JsonProperty("pages")]
        public int Pages { get; set; }

        [JsonPropertyName("page")]
        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonPropertyName("count")]
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonPropertyName("total")]
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonPropertyName("msg")]
        [JsonProperty("msg")]
        public object Msg { get; set; }
    }

    public class SeasonMatchesDetails
    {
        [JsonPropertyName("data")]
        [JsonProperty("data")]
        public List<Datum> Data { get; set; }

        [JsonPropertyName("meta")]
        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

    public class Scores
    {
        [JsonPropertyName("home_score")]
        [JsonProperty("home_score")]
        public string HomeScore { get; set; }

        [JsonPropertyName("away_score")]
        [JsonProperty("away_score")]
        public string AwayScore { get; set; }

        [JsonPropertyName("ht_score")]
        [JsonProperty("ht_score")]
        public string HtScore { get; set; }

        [JsonPropertyName("ft_score")]
        [JsonProperty("ft_score")]
        public string FtScore { get; set; }

        [JsonPropertyName("et_score")]
        [JsonProperty("et_score")]
        public object EtScore { get; set; }

        [JsonPropertyName("ps_score")]
        [JsonProperty("ps_score")]
        public object PsScore { get; set; }
    }

    public class Standings
    {
        [JsonPropertyName("home_position")]
        [JsonProperty("home_position")]
        public int HomePosition { get; set; }

        [JsonPropertyName("away_position")]
        [JsonProperty("away_position")]
        public int AwayPosition { get; set; }
    }

    public class Teams
    {

        [JsonPropertyName("home")]
        [JsonProperty("home")]
        public Home Home { get; set; }

        [JsonPropertyName("away")]
        [JsonProperty("away")]
        public Away Away { get; set; }
    }

    public class Time
    {
        [JsonPropertyName("datetime")]
        [JsonProperty("datetime")]
        public string Datetime { get; set; }

        [JsonPropertyName("date")]
        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonPropertyName("time")]
        [JsonProperty("time")]
        public string TTime { get; set; }

        [JsonPropertyName("minute")]
        [JsonProperty("minute")]
        public int? Minute { get; set; }

        [JsonPropertyName("timestamp")]
        [JsonProperty("timestamp")]
        public int Timestamp { get; set; }

        [JsonPropertyName("timezone")]
        [JsonProperty("timezone")]
        public string Timezone { get; set; }
    }

}
