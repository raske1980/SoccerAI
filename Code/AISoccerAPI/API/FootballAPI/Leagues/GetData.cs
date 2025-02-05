using AISoccerAPI.Calculation;
using AISoccerAPI.Consts;
using AISoccerAPI.Data;
using AISoccerAPI.Serialization;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AISoccerAPI.API.FootballAPI.Leagues
{
    public class GetData
    {
        public async Task GetAPIData(AppConfig appConfig)
        {

            #region Get API Data

            //for new matches, Football API, get all leagues
            var client = new RestClient("https://v3.football.api-sports.io/leagues");
            var request = new RestRequest();
            request.AddHeader("x-rapidapi-key", appConfig.FootballAPIConfig.Key);
            request.AddHeader("x-rapidapi-host", appConfig.FootballAPIConfig.APIUrl);
            RestResponse response = client.Execute(request);
            var apiLeagueDetailResponses = JsonConvert.DeserializeObject<FootballAPILeaguesResponse>(response.Content);
            var apiFootballLeagues = apiLeagueDetailResponses.Response.FindAll(x => x.League.Type == "League").ToList();

            //create list of exclusions (leagues that we are getting through Soccer API)
            var footballAPIMatchExclusions = new List<int>();
            var soccerApiExclusions = new Exclusions().GetSoccerApiLeaaguesByCountry();
            foreach (var soccerAPIExclusion in soccerApiExclusions)
            {
                var matchedLeague = apiLeagueDetailResponses.Response.FirstOrDefault(x =>
                                                                                     x.League.Name.ToLower().Trim() == soccerAPIExclusion.league.ToLower().Trim() &&
                                                                                     x.Country.Name.ToLower().Trim() == soccerAPIExclusion.country.ToLower().Trim());
                if (matchedLeague == null)
                    matchedLeague = apiLeagueDetailResponses.Response.FirstOrDefault(x =>
                                                                                     x.League.Name.ToLower().Trim() == soccerAPIExclusion.footballAPIMappingName.ToLower().Trim() &&
                                                                                     x.Country.Name.ToLower().Trim() == soccerAPIExclusion.country.ToLower().Trim());

                if (matchedLeague != null)
                    footballAPIMatchExclusions.Add(matchedLeague.League.Id);
            }

            var obtainedLeagueIds = await GetObtainedLeagueIds(appConfig);

            var existingMatchFeatures = new CSVSerialization().
                LoadFeaturesFromCSV(appConfig.FootballAPIConfig.BaseFolderPath + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName);

            #endregion

            #region Calculate Match Feature for each fixture in each season for each league

            int numberOfLeagues = 0;
            List<MatchFeatures> matchFeatures = new List<MatchFeatures>();
            foreach (var league in apiFootballLeagues)
            {
                if (numberOfLeagues > APIConsts.MaxFootballAPIRequests) break;
                if (footballAPIMatchExclusions.Contains(league.League.Id) || obtainedLeagueIds.Contains(league.League.Id)) continue;

                //get fixtures by season id, https://v3.football.api-sports.io/fixtures?league=39&season=2023
                Dictionary<int, FootbalAPIFixtureResponse> fixturesBySeason = new Dictionary<int, FootbalAPIFixtureResponse>();
                for (int year = 2021; year <= 2023; year++)
                {
                    var clientBySeason = new RestClient($"https://v3.football.api-sports.io/fixtures?league={league.League.Id}&season={year}");
                    var requestBySeason = new RestRequest();
                    requestBySeason.AddHeader("x-rapidapi-key", appConfig.FootballAPIConfig.Key);
                    requestBySeason.AddHeader("x-rapidapi-host", appConfig.FootballAPIConfig.APIUrl);
                    RestResponse responseBySeason = clientBySeason.Execute(requestBySeason);
                    var fixtureResponse = JsonConvert.DeserializeObject<FootbalAPIFixtureResponse>(responseBySeason.Content);
                    fixturesBySeason.Add(year, fixtureResponse);
                }

                foreach (var keyValuePair in fixturesBySeason)
                {
                    var teamsBySeason = GetTeamsForSeason(keyValuePair.Value);

                    var teamAndPoints = new List<(int teamId, int points)>();
                    teamsBySeason.Keys.ToList().ForEach(x => teamAndPoints.Add((x, 0)));

                    var seasonResponse = keyValuePair.Value.Response.OrderBy(x => x.Fixture.Timestamp).ToList();
                    foreach (var fixture in seasonResponse)
                    {
                        //TO DO - calculate match feature here
                        matchFeatures.Add(new MatchFeatures { });

                        //at the end update points by team
                        Dictionary<int, int> pointsByTeam = CalculatePoints(fixture);
                        foreach (var pointByTeam in pointsByTeam)
                        {
                            var match = teamAndPoints.Find(x => x.teamId == pointByTeam.Key);
                            match.points += pointByTeam.Value;
                        }
                    }
                }

                obtainedLeagueIds.Add(league.League.Id);
                numberOfLeagues++;
            }

            #endregion

            #region SaveData

            existingMatchFeatures.AddRange(matchFeatures);
            new CSVSerialization().
                SaveFeaturesToCsv(existingMatchFeatures, appConfig.FootballAPIConfig.BaseFolderPath + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName);

            await SaveObtainedLeagueIds(obtainedLeagueIds, appConfig);

            #endregion
        }

        private Dictionary<int, int> CalculatePoints(APIFixtureResponse fixture)
        {
            Dictionary<int,int> pointsByTeam = new Dictionary<int,int>();
            pointsByTeam.Add(fixture.Teams.Home.Id, 0);
            pointsByTeam.Add(fixture.Teams.Away.Id, 0);
            if (fixture.Goals.Home > fixture.Goals.Away)
                pointsByTeam[fixture.Teams.Home.Id] = 3;
            else if (fixture.Goals.Away > fixture.Goals.Home)
                pointsByTeam[fixture.Teams.Away.Id] = 3;
            else
            {
                pointsByTeam[fixture.Teams.Home.Id] = 1;
                pointsByTeam[fixture.Teams.Away.Id] = 1;
            }
            return pointsByTeam;
        }

        public Dictionary<int, string> GetTeamsForSeason(FootbalAPIFixtureResponse response)
        {
            Dictionary<int, string> toReturn = new Dictionary<int, string>();

            response.Response.ForEach(x => {
                if (!toReturn.ContainsKey(x.Teams.Home.Id)) toReturn.Add(x.Teams.Home.Id, x.Teams.Home.Name);
                if (!toReturn.ContainsKey(x.Teams.Away.Id)) toReturn.Add(x.Teams.Away.Id, x.Teams.Away.Name);
            });

            return toReturn;
        }

        public async Task<List<int>> GetObtainedLeagueIds(AppConfig appConfig)
        {
            var toReturn = new List<int>();

            var filePath = appConfig.FootballAPIConfig.BaseFolderPath + appConfig.FootballAPIConfig.ObtaindLeagueIDsFileName;
            if(File.Exists(filePath))
            {
                var fileText = await File.ReadAllTextAsync(filePath);
                var obtainedLeagueIds = new List<string>(fileText.Split(new char[1] { ',' })).
                    ConvertAll(x=> { 
                        int parsedLeagueId = 0; 
                        Int32.TryParse(x, out parsedLeagueId); 
                        return parsedLeagueId; 
                    });
                toReturn = obtainedLeagueIds;
            }

            return toReturn;
        }

        public async Task SaveObtainedLeagueIds(List<int> obtainedLeagueIds, AppConfig appConfig)
        {
            var filePath = appConfig.FootballAPIConfig.BaseFolderPath + appConfig.FootballAPIConfig.ObtaindLeagueIDsFileName;
            if(File.Exists(filePath)) 
                File.Delete(filePath);

            string textToSave = string.Empty;
            obtainedLeagueIds.ForEach(x=> textToSave += x.ToString() + ",");
            textToSave =  textToSave.Trim(new char[1] { ',' });

            await File.WriteAllTextAsync(filePath, textToSave);
        }
    }
}
