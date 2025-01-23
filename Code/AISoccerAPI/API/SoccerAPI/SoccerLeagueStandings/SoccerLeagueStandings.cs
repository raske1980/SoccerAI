using AISoccerAPI.API.SoccerAPI.SoccerLeaguesDetail;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AISoccerAPI.API.SoccerAPI.SoccerLeagueStandings
{
    public class SoccerLeagueStanding
    {
        public async Task<SoccerAPIStandingsResponse> GetStandingAsync(string user, string token, string seasonId)
        {
            try
            {
                HttpClient client = new HttpClient();
                string url = $"https://api.soccersapi.com/v2.2/leagues/?user={user}&token={token}&t=standings&season_id={seasonId}";
                HttpResponseMessage response = await client.GetAsync(url);               
                var apiLeagueResponses = JsonConvert.DeserializeObject<SoccerAPIStandingsResponse>(await response.Content.ReadAsStringAsync());
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

    public class SoccerAPIStandingsResponse
    {
        public Data data { get; set; }
        public Meta meta { get; set; }
    }

    public class Away
    {
        public int games_played { get; set; }
        public int won { get; set; }
        public int draw { get; set; }
        public int lost { get; set; }
        public int goals_diff { get; set; }
        public int goals_scored { get; set; }
        public int goals_against { get; set; }
        public int points { get; set; }
        public int position { get; set; }
    }

    public class Data
    {
        public int league_id { get; set; }
        public int season_id { get; set; }
        public int has_groups { get; set; }
        public int number_standings { get; set; }
        public List<Standing> standings { get; set; }
    }

    public class Home
    {
        public int games_played { get; set; }
        public int won { get; set; }
        public int draw { get; set; }
        public int lost { get; set; }
        public int goals_diff { get; set; }
        public int goals_scored { get; set; }
        public int goals_against { get; set; }
        public int points { get; set; }
        public int position { get; set; }
    }

    public class Meta
    {
        public int requests_left { get; set; }
        public string user { get; set; }
        public string plan { get; set; }
        public int pages { get; set; }
        public int page { get; set; }
        public int count { get; set; }
        public int total { get; set; }
        public object msg { get; set; }
    }

    public class Overall
    {
        public int games_played { get; set; }
        public int won { get; set; }
        public int draw { get; set; }
        public int lost { get; set; }
        public int goals_diff { get; set; }
        public int goals_scored { get; set; }
        public int goals_against { get; set; }
        public int points { get; set; }
        public int position { get; set; }
    }    

    public class Standing
    {
        public int team_id { get; set; }
        public string team_name { get; set; }
        public string group { get; set; }
        public string group_name { get; set; }
        public Overall overall { get; set; }
        public Home home { get; set; }
        public Away away { get; set; }
        public int total { get; set; }
        public string status { get; set; }
        public string result { get; set; }
        public int points { get; set; }
        public string recent_form { get; set; }
    }

}
