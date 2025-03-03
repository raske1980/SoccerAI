using AISoccerAPI.Calculation;
using AISoccerAPI.Calculation.SoccerAPI;
using AISoccerAPI.Consts;
using AISoccerAPI.Data;
using AISoccerAPI.JSON.FootballJSON;
using AISoccerAPI.JSON.Merge.Data;
using AISoccerAPI.JSON.OpenData;
using AISoccerAPI.JSON.OpenData.Data;
using AISoccerAPI.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AISoccerAPI.JSON.Merge
{
    public class Merge
    {
        #region Properties

        #endregion

        #region Consts

        private const int OpenDataRandomLimit = 3000;
        private const int FootballJSONRandomLimit = 150000;

        #endregion

        #region Constructors

        public Merge()
        {

        }

        #endregion

        #region Public Methods

        #region Merge/Transform

        public async Task StartMergeAll(AppConfig appConfig)
        {
            //collect API data
            await StartMergeAPI(appConfig);

            //collect JSON data
            StartMergeJSON(appConfig);

            //merge features from different sources
            new MergeMultipleSources().MergeFeatures(appConfig);
        }

        public void StartMergeJSON(AppConfig appConfig)
        {
            var openData = new OpenDataExtract().OpenDataExtractPrepareData(appConfig);
            var footballJSON = new FootballJSONExtract().PrepareData(appConfig);
            new Merge().MergeAll(openData, footballJSON, appConfig);
        }

        public async Task StartMergeAPI(AppConfig appConfig)
        {
            //Soccer API
            await new PrepareData().PrepareDataForTraining(appConfig);

            //Football API
            //await new AISoccerAPI.API.FootballAPI.PrepareData().GetAPIData(appConfig);        
            await new AISoccerAPI.API.FootballAPI.PrepareData().GetAPIDataForFormMomentum(appConfig);

            //FootballData API
            await new AISoccerAPI.API.FootballData.FootballDataAPI().PrepareData(appConfig);
        }

        #endregion

        #endregion

        #region Private Methods

        private List<JSONMatch> TransformOpenDataMatches(Dictionary<string, List<(int competitionId, List<OpenData.Data.Match> matches)>> openDataJSON,
            AppConfig appConfig,
            List<int> openDataMatchIds)
        {
            List<JSONMatch> toReturn = new List<JSONMatch>();
            //iterate through open data matches
            var openDataCompetitions = new OpenDataExtract().GetCompetitions(appConfig);
            foreach (var keyValuePair in openDataJSON)
            {
                foreach (var item in keyValuePair.Value)
                {
                    var competition = openDataCompetitions.FirstOrDefault(x => x.CompetitionId == item.competitionId);
                    foreach (var match in item.matches)
                    {
                        var matchDate = DateTime.MinValue;
                        DateTime.TryParse(match.match_date, out matchDate);
                        var matchId = new Random().Next(1, OpenDataRandomLimit);
                        while (openDataMatchIds.Contains(matchId))
                            matchId = new Random().Next(1, OpenDataRandomLimit);
                        openDataMatchIds.Add(matchId);

                        toReturn.Add(new JSONMatch
                        {
                            AwayTeam = match.away_team.away_team_name,
                            AwayTeamGoals = match.away_score,
                            HomeTeam = match.home_team.home_team_name,
                            HomeTeamGoals = match.home_score,
                            MatchPlayed = matchDate,
                            Season = match.season.season_name,
                            Country = keyValuePair.Key,
                            Competition = match.competition.competition_name,
                            MatchId = matchId
                        });
                    }
                }
            }

            return toReturn;
        }

        private List<JSONMatch> TransformFootballJSONMatches(Dictionary<string, List<(string competition, List<AISoccerAPI.JSON.FootballJSON.Data.MatchExt> matches)>> footballJSON,
            Dictionary<string, List<(string leagueOpenData, string leagueFootballJSON)>> leagueMappings,
            List<string> leagueNames,
            List<int> openDataMatchIds)
        {
            List<JSONMatch> toReturn = new List<JSONMatch>();

            //iterate through FootballJSON matches
            List<int> footballJSONMatchIds = new List<int>();
            foreach (var keyValuePair in footballJSON)
            {
                var country = keyValuePair.Key;

                foreach (var item in keyValuePair.Value)
                {
                    foreach (var match in item.matches)
                    {
                        var matchDate = DateTime.MinValue;
                        DateTime.TryParse(match.date, out matchDate);
                        var matchId = new Random().Next(OpenDataRandomLimit, FootballJSONRandomLimit);
                        while (openDataMatchIds.Contains(matchId))
                            matchId = new Random().Next(OpenDataRandomLimit + 1, FootballJSONRandomLimit);
                        openDataMatchIds.Add(matchId);
                        if (toReturn.FirstOrDefault(x => x.Country == country &&
                                                        x.HomeTeam == match.team1 &&
                                                        x.AwayTeam == match.team2) == null &&
                                                        match.score != null && match.score.ft != null)
                        {
                            string mappedCompetitionName = leagueMappings.ContainsKey(keyValuePair.Key) ?
                                        (leagueMappings[keyValuePair.Key].Where(x => x.leagueOpenData == item.competition).Count() > 0 ?
                                            leagueMappings[keyValuePair.Key].FirstOrDefault(x => x.leagueOpenData == item.competition).leagueFootballJSON : string.Empty) :
                                            string.Empty;

                            string seasonName = match.season;
                            foreach (var competition in leagueNames)
                                seasonName = seasonName.Replace(competition, string.Empty).Trim();

                            if (string.IsNullOrEmpty(mappedCompetitionName))
                            {
                                var seasonNameList = seasonName.Split(new char[1] { ' ' }).ToList();
                                seasonName = seasonNameList[seasonNameList.Count - 1];
                                for (var i = 0; i < seasonNameList.Count - 1; i++)
                                    mappedCompetitionName += seasonNameList[i] + " ";
                                mappedCompetitionName = mappedCompetitionName.Trim();
                            }

                            toReturn.Add(new JSONMatch
                            {
                                AwayTeam = match.team2,
                                AwayTeamGoals = match.score.ft[1],
                                Competition = mappedCompetitionName,
                                Country = country,
                                HomeTeam = match.team1,
                                HomeTeamGoals = match.score.ft[0],
                                MatchId = matchId,
                                MatchPlayed = matchDate,
                                Season = seasonName
                            });
                        }
                    }
                }
            }

            return toReturn;
        }
        private List<JSONMatch> MergeJSONData(Dictionary<string, List<(int competitionId, List<OpenData.Data.Match> matches)>> openDataJSON,
            Dictionary<string, List<(string competition, List<AISoccerAPI.JSON.FootballJSON.Data.MatchExt> matches)>> footballJSON,
            AppConfig appConfig)
        {
            //this method needs to normalize data from all sources, remove duplicates
            List<JSONMatch> toReturn = new List<JSONMatch>();

            var leagueMappings = GetLeagueMappings();
            var leagueNames = GetLeagueNames();
            List<int> openDataMatchIds = new List<int>();

            toReturn.AddRange(TransformOpenDataMatches(openDataJSON, appConfig, openDataMatchIds));
            toReturn.AddRange(TransformFootballJSONMatches(footballJSON, leagueMappings, leagueNames, openDataMatchIds));

            return toReturn;
        }

        private void MergeAll(Dictionary<string, List<(int competitionId, List<OpenData.Data.Match> matches)>> openDataJSON,
            Dictionary<string, List<(string competition, List<AISoccerAPI.JSON.FootballJSON.Data.MatchExt> matches)>> footballJSON,
            AppConfig appConfig)
        {
            var jsonMatches = MergeJSONData(openDataJSON, footballJSON, appConfig);
            var matchFeatures = GetMatchFeatures(jsonMatches);
            SaveMatchFeatures(matchFeatures, appConfig);
        }

        private void SaveMatchFeatures(List<MatchFeatures> matchFeatures, AppConfig appConfig)
        {
            var jsonFolderPath = new DirectoryInfo(appConfig.OpenDataConfig.BaseFolderPath).Parent.FullName;
            new CSVSerialization().
                SaveFeaturesToCsv(matchFeatures, jsonFolderPath + "\\" + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName);
        }

        private List<MatchFeatures> GetMatchFeatures(List<JSONMatch> matches)
        {
            List<MatchFeatures> matchFeatures = new List<MatchFeatures>();

            //grouping by country and competition
            var countryList = matches.Select(x => x.Country).Distinct().ToList();
            Dictionary<string, List<string>> competitonsByCountry = new Dictionary<string, List<string>>();
            foreach (var country in countryList)
                competitonsByCountry.Add(country,
                    matches.Where(x => x.Country == country).Select(x => x.Competition).Distinct().ToList());

            foreach (var keyValuePair in competitonsByCountry)
            {
                foreach (var competition in keyValuePair.Value)
                {
                    var competitionMatches = matches.Where(x =>
                                                                x.Country == keyValuePair.Key &&
                                                                x.Competition == competition).
                                                                ToList();
                    var seasons = competitionMatches.Select(x => x.Season).Distinct().ToList();
                    foreach (var season in seasons)
                    {
                        var seasonMatches = competitionMatches.Where(x => x.Season == season).ToList();
                        seasonMatches = seasonMatches.OrderBy(x => x.MatchPlayed).ToList();
                        seasonMatches = RemoveDuplicates(seasonMatches);
                        List<string> clubs = GetClubs(seasonMatches);
                        var standings = clubs.ConvertAll(x => (x, 0)).ToList();
                        standings = standings.OrderByDescending(x => x.Item2).ToList();
                        foreach (var seasonMatch in seasonMatches)
                        {
                            int homePosition = 0;
                            int awayPosition = 0;
                            if (standings.FindAll(x=>x.Item2 == 0).Count != standings.Count)
                            {
                                homePosition = standings.FindIndex(x => x.x == seasonMatch.HomeTeam) + 1;
                                awayPosition = standings.FindIndex(x => x.x == seasonMatch.AwayTeam) + 1;
                            }                            

                            var winRate = CalculateWinRate(seasonMatch, seasonMatches);
                            var goalDiff = CalculateGoalDifference(seasonMatches, seasonMatch);
                            double formMomentumHome = CalculateFormMomentum(seasonMatches, seasonMatch.HomeTeam);
                            double formMomentumAway = CalculateFormMomentum(seasonMatches, seasonMatch.AwayTeam);

                            matchFeatures.Add(new MatchFeatures
                            {
                                MatchId = seasonMatch.MatchId,
                                HomeTeam = seasonMatch.HomeTeam,
                                AwayTeam = seasonMatch.AwayTeam,
                                Date = seasonMatch.MatchPlayed.ToString("dd/MM/yyyy"),
                                GoalDifference = goalDiff.homePastAvg - goalDiff.awayPastAvg,
                                WinRateAway = winRate.winRateAway,
                                WinRateHome = winRate.winRateHome,
                                FormMomentumHome = formMomentumHome,
                                FormMomentumAway = formMomentumAway,
                                LeagueRankDifference = homePosition - awayPosition,
                                HomeGoals = seasonMatch.HomeTeamGoals,
                                AwayGoals = seasonMatch.AwayTeamGoals,
                            });

                            Dictionary<string, int> pointsByTeam = CalculatePoints(seasonMatch);
                            foreach (var pointByTeam in pointsByTeam)
                            {
                                var match = standings.Find(x => x.x == pointByTeam.Key);
                                match.Item2 += pointByTeam.Value;
                            }
                            standings = standings.OrderByDescending(x => x.Item2).ToList();
                        }
                    }
                }
            }

            return matchFeatures;
        }

        private Dictionary<string, int> CalculatePoints(JSONMatch match)
        {
            Dictionary<string, int> pointsByTeam = new Dictionary<string, int>();
            pointsByTeam.Add(match.HomeTeam, 0);
            pointsByTeam.Add(match.AwayTeam, 0);
            if (match.HomeTeamGoals > match.AwayTeamGoals)
                pointsByTeam[match.HomeTeam] = 3;
            else if (match.AwayTeamGoals > match.HomeTeamGoals)
                pointsByTeam[match.AwayTeam] = 3;
            else
            {
                pointsByTeam[match.HomeTeam] = 1;
                pointsByTeam[match.AwayTeam] = 1;
            }
            return pointsByTeam;
        }

        private double CalculateFormMomentum(List<JSONMatch> seasonMatches, string team)
        {
            var lastMatchesOfTeam = seasonMatches.Where(x =>
                                                          x.HomeTeam == team ||
                                                          x.AwayTeam == team).
                                                          Skip(0).Take(APIConsts.FormMomentumMax).ToList();

            lastMatchesOfTeam = lastMatchesOfTeam.OrderByDescending(x => x.MatchPlayed).ToList();

            double sumOfPoints = 0;
            double sumOfWeights = 0;
            var listOfWeights = CalculateSoccerAPI.GetWeights();
            for (var i = 0; i < lastMatchesOfTeam.Count; i++)
            {
                double weight = listOfWeights[i];

                sumOfWeights += weight;
                var isHomeTeam = lastMatchesOfTeam[i].HomeTeam == team ? true : false;

                int parseHomeScore = lastMatchesOfTeam[i].HomeTeamGoals;
                int parseAwayScore = lastMatchesOfTeam[i].AwayTeamGoals;
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

        private (double homePastAvg, double awayPastAvg) CalculateGoalDifference(List<JSONMatch> seasonMatches, JSONMatch match)
        {
            var pastHomeMatches = seasonMatches.Where(x =>
                                      x.HomeTeam == match.HomeTeam &&
                                      x.MatchPlayed < match.MatchPlayed).ToList();
            var scoredHomeGoals = pastHomeMatches.Select(x => x.HomeTeamGoals).Sum();

            var pastAwayMatches = seasonMatches.Where(x =>
                                  x.AwayTeam == match.AwayTeam &&
                                  x.MatchPlayed < match.MatchPlayed).ToList();
            var scoredAwayGoals = pastAwayMatches.Select(x => x.AwayTeamGoals).Sum();

            double homePastAvg = pastHomeMatches.Count > 0 ? (double)scoredHomeGoals / (double)pastHomeMatches.Count : 0;
            double awayPastAvg = pastAwayMatches.Count > 0 ? (double)scoredAwayGoals / (double)pastAwayMatches.Count : 0;

            return (homePastAvg, awayPastAvg);
        }

        private (double winRateHome, double winRateAway) CalculateWinRate(JSONMatch match, List<JSONMatch> seasonMatches)
        {
            var previousGames = seasonMatches.FindAll(x => x.MatchPlayed < match.MatchPlayed);

            var previousHomeGames = previousGames.FindAll(x => x.HomeTeam == match.HomeTeam);
            var winHomeGames = previousHomeGames.FindAll(x => x.HomeTeamGoals > x.AwayTeamGoals);

            var previousAwayGames = previousGames.FindAll(x => x.AwayTeam == match.AwayTeam);
            var winAwayGames = previousHomeGames.FindAll(x => x.AwayTeamGoals > x.HomeTeamGoals);

            double winRateHome = ((double)winHomeGames.Count / (double)previousHomeGames.Count) * 100d;
            double winRateAway = ((double)winAwayGames.Count / (double)previousAwayGames.Count) * 100d;

            return (winRateHome, winRateAway);
        }

        private List<string> GetClubs(List<JSONMatch> seasonMatches)
        {
            List<string> clubs = new List<string>();

            var homeTeams = seasonMatches.Select(x => x.HomeTeam).Distinct().ToList();
            var awayTeams = seasonMatches.Select(x => x.AwayTeam).Distinct().ToList();

            foreach (var team in homeTeams)
                if (!clubs.Contains(team))
                    clubs.Add(team);

            foreach (var team in awayTeams)
                if (!clubs.Contains(team))
                    clubs.Add(team);

            return clubs;
        }

        private List<JSONMatch> RemoveDuplicates(List<JSONMatch> matches)
        {
            List<JSONMatch> duplicates = new List<JSONMatch>();
            foreach(var match in matches)
                if(matches.FindAll(x=>x.MatchId != match.MatchId && 
                                        x.HomeTeam == match.HomeTeam && 
                                        x.AwayTeam == match.AwayTeam && 
                                        x.MatchPlayed.Day == match.MatchPlayed.Day && 
                                        x.MatchPlayed.Month == match.MatchPlayed.Month && 
                                        x.MatchPlayed.Year == match.MatchPlayed.Year).Count > 0)
                {
                    duplicates.AddRange(matches.FindAll(x => x.MatchId != match.MatchId &&
                                        x.HomeTeam == match.HomeTeam &&
                                        x.AwayTeam == match.AwayTeam &&
                                        x.MatchPlayed.Day == match.MatchPlayed.Day &&
                                        x.MatchPlayed.Month == match.MatchPlayed.Month &&
                                        x.MatchPlayed.Year == match.MatchPlayed.Year));
                }

            foreach (var duplicate in duplicates)
                matches.Remove(duplicate);

            return matches;
        }

        private Dictionary<string, List<(string leagueOpenData, string leagueFootballJSON)>> GetLeagueMappings()
        {
            Dictionary<string, List<(string leagueOpenData, string leagueFootballJSON)>> leagueMappings =
                new Dictionary<string, List<(string leagueOpenData, string leagueFootballJSON)>>();
            leagueMappings.Add("Germany", new List<(string leagueOpenData, string leagueFootballJSON)> { ("1", "1. Bundesliga") });
            leagueMappings.Add("Spain", new List<(string leagueOpenData, string leagueFootballJSON)> { ("1", "La Liga") });
            leagueMappings.Add("England", new List<(string leagueOpenData, string leagueFootballJSON)> { ("1", "Premier League") });
            leagueMappings.Add("Argentina", new List<(string leagueOpenData, string leagueFootballJSON)> { ("1", "Liga Profesional") });
            leagueMappings.Add("France", new List<(string leagueOpenData, string leagueFootballJSON)> { ("1", "Ligue 1") });
            leagueMappings.Add("Italy", new List<(string leagueOpenData, string leagueFootballJSON)> { ("1", "Serie A") });
            return leagueMappings;
        }

        private List<string> GetLeagueNames()
        {
            List<string> leagueNames = new List<string> { "1. Bundesliga", "La Liga", "Premier League", "Liga Professional", "Ligue 1", "Serie A" };
            return leagueNames;
        }

        #endregion

    }
}
