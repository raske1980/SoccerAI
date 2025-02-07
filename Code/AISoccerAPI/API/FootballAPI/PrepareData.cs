using AISoccerAPI.API.SoccerAPI.SoccerSeasonMathesDetail;
using AISoccerAPI.Calculation;
using AISoccerAPI.Calculation.SoccerAPI;
using AISoccerAPI.Consts;
using AISoccerAPI.Data;
using AISoccerAPI.Serialization;
using Microsoft.Extensions.FileSystemGlobbing;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AISoccerAPI.API.FootballAPI
{
    public class PrepareData
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
                    Thread.Sleep(6100);//added because of football api free plan limitation
                }

                foreach (var keyValuePair in fixturesBySeason)
                {
                    var teamsBySeason = GetTeamsForSeason(keyValuePair.Value);

                    var teamAndPoints = new List<(int teamId, int points)>();
                    teamsBySeason.Keys.ToList().ForEach(x => teamAndPoints.Add((x, 0)));

                    var seasonResponse = keyValuePair.Value.Response.OrderBy(x => x.Fixture.Timestamp).ToList();
                    foreach (var fixture in seasonResponse)
                    {
                        int homePosition = 0;
                        int awayPosition = 0;
                        if(teamAndPoints.Where(x=>x.points == 0).Count() != teamAndPoints.Count)
                        {
                            var homeIndex = teamAndPoints.FindIndex(x => x.teamId == fixture.Teams.Home.Id);
                            var awayIndex = teamAndPoints.FindIndex(x => x.teamId == fixture.Teams.Away.Id);
                            homePosition = homeIndex + 1;
                            awayPosition = awayIndex + 1;
                        }

                        var winRate = CalculateWinRate(fixture, seasonResponse);
                        var goalDiff = CalculateGoalDifference(seasonResponse, fixture);
                        double formMomentumHome = CalculateFormMomentum(seasonResponse, fixture.Teams.Home.Id);
                        double formMomentumAway = CalculateFormMomentum(seasonResponse, fixture.Teams.Away.Id);
                        
                        matchFeatures.Add(new MatchFeatures {
                            MatchId = fixture.Fixture.Id,
                            HomeTeam = fixture.Teams.Home.Name,
                            AwayTeam = fixture.Teams.Away.Name,
                            Date = new DateTime(fixture.Fixture.Timestamp).ToString("dd/MM/yyyy"),
                            GoalDifference = goalDiff.homePastAvg - goalDiff.awayPastAvg,
                            WinRateAway = winRate.winRateAway,
                            WinRateHome = winRate.winRateHome,
                            FormMomentumHome = formMomentumHome,
                            FormMomentumAway = formMomentumAway,
                            LeagueRankDifference = homePosition - awayPosition,
                            HomeGoals = fixture.Goals.Home.HasValue ? fixture.Goals.Home.Value : 0,
                            AwayGoals = fixture.Goals.Away.HasValue ? fixture.Goals.Away.Value : 0,
                        });

                        //at the end update points by team and sort list
                        Dictionary<int, int> pointsByTeam = CalculatePoints(fixture);
                        foreach (var pointByTeam in pointsByTeam)
                        {
                            var match = teamAndPoints.Find(x => x.teamId == pointByTeam.Key);
                            match.points += pointByTeam.Value;
                        }
                        teamAndPoints = teamAndPoints.OrderByDescending(x=>x.points).ToList();
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

        #region Private Methods

        private double CalculateFormMomentum(List<APIFixtureResponse> matches, int teamId)
        {
            var lastMatchesOfTeam = matches.Where(x =>
                                                          x.Teams.Home.Id == teamId ||
                                                          x.Teams.Away.Id == teamId).
                                                          Skip(0).Take(APIConsts.FormMomentumMax).ToList();

            lastMatchesOfTeam = lastMatchesOfTeam.OrderBy(x => x.Fixture.Timestamp).ToList();

            double sumOfPoints = 0;
            double sumOfWeights = 0;
            var listOfWeights = CalculateSoccerAPI.GetWeights();
            for (var i = 0; i < lastMatchesOfTeam.Count; i++)
            {
                double weight = listOfWeights[i];

                sumOfWeights += weight;
                var isHomeTeam = lastMatchesOfTeam[i].Teams.Home.Id == teamId ? true : false;

                int parseHomeScore = lastMatchesOfTeam[i].Goals.Home.HasValue ? lastMatchesOfTeam[i].Goals.Home.Value : 0;
                int parseAwayScore = lastMatchesOfTeam[i].Goals.Away.HasValue ? lastMatchesOfTeam[i].Goals.Away.Value : 0; 
                if (isHomeTeam)
                    sumOfPoints += weight *
                        (parseHomeScore > parseAwayScore ?
                        APIConsts.Win :
                        (parseHomeScore == parseAwayScore ?
                        APIConsts.Draw : APIConsts.Lost));
                else
                    sumOfPoints += weight *
                        (parseAwayScore > parseHomeScore ?
                        APIConsts.Win :
                        (parseAwayScore == parseHomeScore ?
                        APIConsts.Draw : APIConsts.Lost));
            }

            double formMomentum = sumOfPoints / sumOfWeights;
            return formMomentum;
        }

        private (double homePastAvg, double awayPastAvg) CalculateGoalDifference(List<APIFixtureResponse> matches, APIFixtureResponse match)
        {
            var pastHomeMatches = matches.Where(x =>
                                      x.Teams.Home.Id == match.Teams.Home.Id &&
                                      new DateTime(x.Fixture.Timestamp) < new DateTime(match.Fixture.Timestamp)).ToList();
            var scoredHomeGoals = pastHomeMatches.Select(x => x.Goals.Home).Sum();

            var pastAwayMatches = matches.Where(x =>
                                  x.Teams.Away.Id == match.Teams.Away.Id &&
                                  new DateTime(x.Fixture.Timestamp) < new DateTime(match.Fixture.Timestamp)).ToList();
            var scoredAwayGoals = pastAwayMatches.Select(x => x.Goals.Away).Sum();

            double homePastAvg = pastHomeMatches.Count > 0 ? (double)scoredHomeGoals / (double)pastHomeMatches.Count : 0;
            double awayPastAvg = pastAwayMatches.Count > 0 ? (double)scoredAwayGoals / (double)pastAwayMatches.Count : 0;

            return (homePastAvg, awayPastAvg);
        }

        private (double winRateHome, double winRateAway) CalculateWinRate(APIFixtureResponse fixture, List<APIFixtureResponse> seasonFixtures)
        {
            var previousGames = seasonFixtures.FindAll(x=>x.Fixture.Timestamp < fixture.Fixture.Timestamp);

            var previousHomeGames = previousGames.FindAll(x=>x.Teams.Home.Id == fixture.Teams.Home.Id);
            var winHomeGames = previousHomeGames.FindAll(x=>x.Goals.Home > x.Goals.Away);

            var previousAwayGames = previousGames.FindAll(x => x.Teams.Away.Id == fixture.Teams.Away.Id);
            var winAwayGames = previousHomeGames.FindAll(x => x.Goals.Away > x.Goals.Home);

            double winRateHome = ((double)winHomeGames.Count / (double)previousHomeGames.Count) * 100d;
            double winRateAway = ((double)winAwayGames.Count / (double)previousAwayGames.Count) * 100d;

            return (winRateHome, winRateAway);
        }

        private Dictionary<int, int> CalculatePoints(APIFixtureResponse fixture)
        {
            Dictionary<int, int> pointsByTeam = new Dictionary<int, int>();
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

        private Dictionary<int, string> GetTeamsForSeason(FootbalAPIFixtureResponse response)
        {
            Dictionary<int, string> toReturn = new Dictionary<int, string>();

            response.Response.ForEach(x =>
            {
                if (!toReturn.ContainsKey(x.Teams.Home.Id)) toReturn.Add(x.Teams.Home.Id, x.Teams.Home.Name);
                if (!toReturn.ContainsKey(x.Teams.Away.Id)) toReturn.Add(x.Teams.Away.Id, x.Teams.Away.Name);
            });

            return toReturn;
        }

        private async Task<List<int>> GetObtainedLeagueIds(AppConfig appConfig)
        {
            var toReturn = new List<int>();

            var filePath = appConfig.FootballAPIConfig.BaseFolderPath + appConfig.FootballAPIConfig.ObtaindLeagueIDsFileName;
            if (File.Exists(filePath))
            {
                var fileText = await File.ReadAllTextAsync(filePath);
                var obtainedLeagueIds = new List<string>(fileText.Split(new char[1] { ',' })).
                    ConvertAll(x =>
                    {
                        int parsedLeagueId = 0;
                        int.TryParse(x, out parsedLeagueId);
                        return parsedLeagueId;
                    });
                toReturn = obtainedLeagueIds;
            }

            return toReturn;
        }

        private async Task SaveObtainedLeagueIds(List<int> obtainedLeagueIds, AppConfig appConfig)
        {
            var filePath = appConfig.FootballAPIConfig.BaseFolderPath + appConfig.FootballAPIConfig.ObtaindLeagueIDsFileName;
            if (File.Exists(filePath))
                File.Delete(filePath);

            string textToSave = string.Empty;
            obtainedLeagueIds.ForEach(x => textToSave += x.ToString() + ",");
            textToSave = textToSave.Trim(new char[1] { ',' });

            await File.WriteAllTextAsync(filePath, textToSave);
        }

        #endregion
    }
}
