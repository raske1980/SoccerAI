using AISoccerAPI.Calculation;
using AISoccerAPI.Data;
using AISoccerAPI.JSON.Merge.Data;
using AISoccerAPI.JSON.OpenData;
using AISoccerAPI.JSON.OpenData.Data;
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

        public const int OpenDataRandomLimit = 3000;
        public const int FootballJSONRandomLimit = 150000;

        #endregion


        #region Constructors

        public Merge()
        {

        }

        #endregion

        #region Public Methods

        public List<MatchFeatures> MergeAll(Dictionary<string, List<(int competitionId, List<OpenData.Data.Match> matches)>> openDataJSON,
            Dictionary<string, List<(string competition, List<AISoccerAPI.JSON.FootballJSON.Data.MatchExt> matches)>> footballJSON,
            AppConfig appConfig)
        {
            var jsonMatches = MergeJSONData(openDataJSON, footballJSON, appConfig);
            return GetMatchFeatures(jsonMatches);
        }

        public List<JSONMatch> MergeJSONData(Dictionary<string, List<(int competitionId, List<OpenData.Data.Match> matches)>> openDataJSON,
            Dictionary<string, List<(string competition, List<AISoccerAPI.JSON.FootballJSON.Data.MatchExt> matches)>> footballJSON,
            AppConfig appConfig)
        {
            //this method needs to normalize data from all sources, remove duplicates
            List<JSONMatch> toReturn = new List<JSONMatch>();

            Dictionary<string, List<(string leagueOpenData, string leagueFootballJSON)>> leagueMappings = 
                new Dictionary<string, List<(string leagueOpenData, string leagueFootballJSON)>>();
            leagueMappings.Add("Germany", new List<(string leagueOpenData, string leagueFootballJSON)> { ("1", "1. Bundesliga") });
            leagueMappings.Add("Spain", new List<(string leagueOpenData, string leagueFootballJSON)> { ("1", "La Liga") });
            leagueMappings.Add("England", new List<(string leagueOpenData, string leagueFootballJSON)> { ("1", "Premier League") });
            leagueMappings.Add("Argentina", new List<(string leagueOpenData, string leagueFootballJSON)> { ("1", "Liga Profesional") });
            leagueMappings.Add("France", new List<(string leagueOpenData, string leagueFootballJSON)> { ("1", "Ligue 1") });
            leagueMappings.Add("Italy", new List<(string leagueOpenData, string leagueFootballJSON)> { ("1", "Serie A") });
            List<string> leagueNames = new List<string> { "1. Bundesliga", "La Liga", "Premier League", "Liga Professional", "Ligue 1", "Serie A" };

            //iterate through open data matches
            var openDataCompetitions = new OpenDataExtract().GetCompetitions(appConfig);
            List<int> openDataMatchIds = new List<int>();
            foreach(var keyValuePair in openDataJSON)
            {
                foreach(var item in keyValuePair.Value)
                {
                    var competition = openDataCompetitions.FirstOrDefault(x => x.CompetitionId == item.competitionId);
                    foreach(var match in item.matches)
                    {
                        var matchDate = DateTime.MinValue;
                        DateTime.TryParse(match.match_date, out matchDate);
                        var matchId = new Random().Next(1, OpenDataRandomLimit);
                        while (openDataMatchIds.Contains(matchId))
                            matchId = new Random().Next(1, OpenDataRandomLimit);
                        openDataMatchIds.Add(matchId);
                        
                        toReturn.Add(new JSONMatch { 
                            AwayTeam = match.away_team.away_team_name,
                            AwayTeamGoals = match.away_score,
                            HomeTeam = match.home_team.home_team_name,
                            HomeTeamGoals = match.home_score,
                            MatchPlayed = matchDate,
                            Season = competition != null ? competition.SeasonName : string.Empty,
                            Country = keyValuePair.Key,
                            Competition = competition.CompetitionName,
                            MatchId = matchId
                        });
                    }
                }                
            }

            //iterate through FootballJSON matches
            List<int> footballJSONMatchIds = new List<int>();            
            foreach(var keyValuePair in footballJSON)
            {
                var country = keyValuePair.Key;
                
                foreach (var item in keyValuePair.Value)
                {
                    foreach(var match in item.matches)
                    {
                        var matchDate = DateTime.MinValue;
                        DateTime.TryParse(match.date, out matchDate);
                        var matchId = new Random().Next(OpenDataRandomLimit, FootballJSONRandomLimit);
                        while (openDataMatchIds.Contains(matchId))
                            matchId = new Random().Next(OpenDataRandomLimit + 1, FootballJSONRandomLimit);
                        openDataMatchIds.Add(matchId);
                        if(toReturn.FirstOrDefault(x=>x.Country == country && 
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

                            if(string.IsNullOrEmpty(mappedCompetitionName))
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

        public List<MatchFeatures> GetMatchFeatures(List<JSONMatch> matches)
        {
            //this method needs to use normalized data and to create
            //match features that will also later when it is merged with
            //API features be filtered from duplicates

            List<MatchFeatures> matchFeatures = new List<MatchFeatures>();

            //grouping by country
            var countryList = matches.Select(x => x.Country).Distinct().ToList();
            Dictionary<string, List<JSONMatch>> matchesByCountry = new Dictionary<string, List<JSONMatch>>();
            foreach (var country in countryList)
                matchesByCountry.Add(country, matches.FindAll(x => x.Country == country));

            //group by competition for each country
            Dictionary<string, List<string>> competitionsByCountry = new Dictionary<string, List<string>>();
            foreach (var keyValuePair in matchesByCountry)
            {
                var competitions = keyValuePair.Value.Where(x => 
                                                                x.Country == keyValuePair.Key).
                                                                Select(x => x.Competition).
                                                                Distinct().
                                                                ToList();
                competitions.ToList().RemoveAll(x => string.IsNullOrEmpty(x));
                competitionsByCountry.Add(keyValuePair.Key, competitions);
            }

            //we will group matches by competitions (list for duplicate name for competitions exist),
            //and when we make union of all duplicate competitions then we will need to group them by season
            //and on season level we will create matchfeatures (because we need to calculate standing manually by season,
            //because we dont have that information)
            foreach (var keyValuePair in matchesByCountry)
            {
                var competitions = competitionsByCountry[keyValuePair.Key];
                foreach (var competition in competitions)
                {
                    var competitionMatches = keyValuePair.Value.FindAll(x => x.Competition == competition);
                }
            }

            return matchFeatures;
        }

        #endregion

        #region Private Methods

        #endregion

    }
}
