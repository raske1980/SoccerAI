using AISoccerAPI.API.FootballData.Data;
using AISoccerAPI.Calculation;
using AISoccerAPI.Calculation.SoccerAPI;
using AISoccerAPI.Consts;
using AISoccerAPI.Data;
using AISoccerAPI.JSON.Merge.Data;
using AISoccerAPI.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Tensorflow;

namespace AISoccerAPI.API.FootballData
{
    public class FootballDataAPI
    {
        #region Properties


        #endregion

        #region Constructors

        public FootballDataAPI()
        {

        }

        #endregion

        #region Methods

        public async Task<List<MatchFeatures>> PrepareData(AppConfig appConfig)
        {
            List<MatchFeatures> toReturn = new List<MatchFeatures>();

            Dictionary<int, List<AISoccerAPI.API.FootballData.Data.MatchRes>> result = new Dictionary<int, List<MatchRes>>();
            for (var i = 2014; i <= 2024; i++)
            {
                var matches = await new FootballDataAPI().GetMatchesForSeasonAPI("BSA", i.ToString(), appConfig);
                result.Add(i, matches);
            }

            foreach(var keyValuePair in result)
            {
                var matches = keyValuePair.Value;
                List<string> clubs = GetClubs(keyValuePair.Value);
                var standings = clubs.ConvertAll(x => (x,0)).ToList();
                standings = standings.OrderByDescending(x => x.Item2).ToList();
                foreach(var match in matches)
                {
                    int homePosition = 0;
                    int awayPosition = 0;
                    if (standings.FindAll(x => x.Item2 == 0).Count != standings.Count)
                    {
                        homePosition = standings.FindIndex(x => x.x == match.HomeTeam.Name) + 1;
                        awayPosition = standings.FindIndex(x => x.x == match.AwayTeam.Name) + 1;

                        var winRate = CalculateWinRate(match, matches);
                        var goalDiff = CalculateGoalDifference(matches, match);
                        double formMomentumHome = CalculateFormMomentum(matches, match.HomeTeam.Name, match.UtcDate);
                        double formMomentumAway = CalculateFormMomentum(matches, match.AwayTeam.Name, match.UtcDate);

                        toReturn.Add(new MatchFeatures
                        {
                            MatchId = new Random().Next(1000000,1000000000),
                            HomeTeam = match.HomeTeam.Name,
                            AwayTeam = match.AwayTeam.Name,
                            Date = DateTime.Parse(match.UtcDate).ToString("dd/MM/yyyy"),
                            GoalDifference = goalDiff.homePastAvg - goalDiff.awayPastAvg,
                            WinRateAway = winRate.winRateAway,
                            WinRateHome = winRate.winRateHome,
                            FormMomentumHome = formMomentumHome,
                            FormMomentumAway = formMomentumAway,
                            LeagueRankDifference = homePosition - awayPosition,
                            HomeGoals = match.Score.FullTime.Home.HasValue ? match.Score.FullTime.Home.Value : 0,
                            AwayGoals = match.Score.FullTime.Away.HasValue ? match.Score.FullTime.Away.Value : 0,
                        });

                        Dictionary<string, int> pointsByTeam = CalculatePoints(match);
                        foreach (var pointByTeam in pointsByTeam)
                        {
                            var foundMatch = standings.Find(x => x.x == pointByTeam.Key);
                            foundMatch.Item2 += pointByTeam.Value;
                        }
                        standings = standings.OrderByDescending(x => x.Item2).ToList();
                    }
                }
            }

            new CSVSerialization().SaveFeaturesToCsv(toReturn, appConfig.FootballDataConfig.BaseFolderPath + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName);

            return toReturn;
        }

        private Dictionary<string, int> CalculatePoints(MatchRes match)
        {
            Dictionary<string, int> pointsByTeam = new Dictionary<string, int>();
            pointsByTeam.Add(match.HomeTeam.Name, 0);
            pointsByTeam.Add(match.AwayTeam.Name, 0);
            if (match.Score.FullTime.Home > match.Score.FullTime.Away)
                pointsByTeam[match.HomeTeam.Name] = 3;
            else if (match.Score.FullTime.Away > match.Score.FullTime.Home)
                pointsByTeam[match.AwayTeam.Name] = 3;
            else
            {
                pointsByTeam[match.HomeTeam.Name] = 1;
                pointsByTeam[match.AwayTeam.Name] = 1;
            }
            return pointsByTeam;
        }

        private double CalculateFormMomentum(List<MatchRes> seasonMatches, string team, string matchDate)
        {
            DateTime parsedDate = DateTime.MinValue;
            DateTime.TryParse(matchDate, out parsedDate);
            var lastMatchesOfTeam = seasonMatches.Where(x =>
                                                          (x.HomeTeam.Name == team ||
                                                          x.AwayTeam.Name == team) && DateTime.Parse(x.UtcDate) < parsedDate).
                                                          ToList();

            lastMatchesOfTeam = lastMatchesOfTeam.OrderByDescending(x => { 
                                                                            DateTime parsedDate = DateTime.MinValue; 
                                                                            DateTime.TryParse(x.UtcDate, out parsedDate); 
                                                                            return parsedDate; }).
                                                                            Skip(0).Take(APIConsts.FormMomentumMax).
                                                                            ToList();

            double sumOfPoints = 0;
            double sumOfWeights = 0;
            var listOfWeights = CalculateSoccerAPI.GetWeights();
            for (var i = 0; i < lastMatchesOfTeam.Count; i++)
            {
                double weight = listOfWeights[i];

                sumOfWeights += weight;
                var isHomeTeam = lastMatchesOfTeam[i].HomeTeam.Name == team ? true : false;

                int parseHomeScore = lastMatchesOfTeam[i].Score.FullTime.Home.HasValue ? lastMatchesOfTeam[i].Score.FullTime.Home.Value : 0;
                int parseAwayScore = lastMatchesOfTeam[i].Score.FullTime.Away.HasValue ? lastMatchesOfTeam[i].Score.FullTime.Away.Value : 0;
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

        private (double homePastAvg, double awayPastAvg) CalculateGoalDifference(List<MatchRes> seasonMatches, MatchRes match)
        {
            var pastHomeMatches = seasonMatches.Where(x =>
                                      x.HomeTeam.Name == match.HomeTeam.Name &&
                                      DateTime.Parse(x.UtcDate) < DateTime.Parse(match.UtcDate)).ToList();
            var scoredHomeGoals = pastHomeMatches.Select(x => x.Score.FullTime.Home).Sum();

            var pastAwayMatches = seasonMatches.Where(x =>
                                  x.AwayTeam.Name == match.AwayTeam.Name &&
                                  DateTime.Parse(x.UtcDate) < DateTime.Parse(match.UtcDate)).ToList();
            var scoredAwayGoals = pastAwayMatches.Select(x => x.Score.FullTime.Away).Sum();

            double homePastAvg = pastHomeMatches.Count > 0 ? (double)scoredHomeGoals / (double)pastHomeMatches.Count : 0;
            double awayPastAvg = pastAwayMatches.Count > 0 ? (double)scoredAwayGoals / (double)pastAwayMatches.Count : 0;

            return (homePastAvg, awayPastAvg);
        }

        private (double winRateHome, double winRateAway) CalculateWinRate(MatchRes match, List<MatchRes> seasonMatches)
        {
            var previousGames = seasonMatches.FindAll(x => DateTime.Parse(x.UtcDate) < DateTime.Parse(match.UtcDate));

            var previousHomeGames = previousGames.FindAll(x => x.HomeTeam.Name == match.HomeTeam.Name);
            var winHomeGames = previousHomeGames.FindAll(x => x.Score.FullTime.Home > x.Score.FullTime.Away);

            var previousAwayGames = previousGames.FindAll(x => x.AwayTeam.Name == match.AwayTeam.Name);
            var winAwayGames = previousHomeGames.FindAll(x => x.Score.FullTime.Away > match.Score.FullTime.Home);

            double winRateHome = ((double)winHomeGames.Count / (double)previousHomeGames.Count) * 100d;
            double winRateAway = ((double)winAwayGames.Count / (double)previousAwayGames.Count) * 100d;

            return (winRateHome, winRateAway);
        }

        private List<string> GetClubs(List<MatchRes> matches)
        {
            List<string> toReturn = new List<string>();

            foreach(var match in matches)
            {
                if (!toReturn.Contains(match.HomeTeam.Name))
                    toReturn.Add(match.HomeTeam.Name);

                if (!toReturn.Contains(match.AwayTeam.Name))
                    toReturn.Add(match.AwayTeam.Name);
            }

            return toReturn;
        }

        private async Task<List<MatchRes>> GetMatchesForSeasonAPI(string countryAbbr, string season, AppConfig appConfig)
        {
            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("X-Auth-Token", appConfig.FootballDataConfig.Key);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                HttpResponseMessage response = await client.GetAsync(string.Format(appConfig.FootballDataConfig.MatchesUrl, countryAbbr, season));
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var matches = JsonSerializer.Deserialize<MatchesResponseRes>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return new List<MatchRes>(matches.Matches);
                }
                else
                {
                    return new List<MatchRes>();
                }
            }
            catch (Exception ex)
            {
                return new List<MatchRes>();
            }
        }

        #endregion
    }
}

