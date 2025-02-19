using AISoccerAPI.Calculation;
using AISoccerAPI.Data;
using AISoccerAPI.JSON.Merge.Data;
using AISoccerAPI.JSON.OpenData;
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
                            Competition = competition != null ? competition.CompetitionName : string.Empty,
                            MatchId = matchId
                        });
                    }
                }                
            }

            List<int> footballJSONMatchIds = new List<int>();
            foreach(var keyValuePair in footballJSON)
            {
                var country = new OpenDataExtract().GetAbbrevationForCountries()[keyValuePair.Key];
                foreach(var item in keyValuePair.Value)
                {
                    foreach(var match in item.matches)
                    {
                        var matchDate = DateTime.MinValue;
                        DateTime.TryParse(match.date, out matchDate);
                        var matchId = new Random().Next(OpenDataRandomLimit, FootballJSONRandomLimit);
                        while (openDataMatchIds.Contains(matchId))
                            matchId = new Random().Next(1, OpenDataRandomLimit);
                        openDataMatchIds.Add(matchId);
                        if(toReturn.FirstOrDefault(x=>x.Country == country && 
                                                        x.HomeTeam == match.team1 && 
                                                        x.AwayTeam == match.team2) == null)
                        {
                            toReturn.Add(new JSONMatch { 
                                AwayTeam = match.team2,
                                AwayTeamGoals = match.score.ft[1],
                                Competition = keyValuePair.Key,
                                Country = country,
                                HomeTeam = match.team1,
                                HomeTeamGoals = match.score.ft[0],
                                MatchId = matchId,
                                MatchPlayed = matchDate,
                                Season = match.season                                
                            });
                        }
                    }                    
                }
            }

            //we have merged 2 json sources here (avoiding duplicates between 2 lists),
            //next step will be eliminate duplicates from this list by using MatchFeatures file,
            //where (despite that we dont have dates now) we will compare name of the teams and final result,
            //and if it is the same then we have duplocate so that means that we will remove it from unique list,
            //and after that we will group matches by country and season (we will need mapping for seasons from one source to the other one)
            //and after that we can create JSON MatchFeature file that will be merged with other files

            return toReturn;
        }

        public List<MatchFeatures> GetMatchFeatures(List<JSONMatch> matches)
        {
            List<MatchFeatures> matchFeatures = new List<MatchFeatures>();
            //this method needs to use normalized data and to create
            //match features that will also later when it is merged with
            //API features be filtered from duplicates
            return matchFeatures;
        }

        #endregion

        #region Private Methods

        #endregion

    }
}
