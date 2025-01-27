using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AISoccerAPI.API.SoccerAPI.SoccerLeagueDetail;
using AISoccerAPI.API.SoccerAPI.SoccerSeasonMathesDetail;
using AISoccerAPI.Consts;
using Microsoft.Extensions.FileSystemGlobbing;

namespace AISoccerAPI.Calculation.SoccerAPI
{
    public class CalculateSoccerAPI
    {

        #region Methods

        public async Task<List<MatchFeatures>> CalculateMatchFeatures(LeagueDetailsResponse leagueDetails, string user, string token)
        {
            List<MatchFeatures> matchFeatures = new List<MatchFeatures>();
            List<SeasonMatchesDetails> seasons = new List<SeasonMatchesDetails>();

            #region API Calls

            //get all matches from all seasons
            foreach (var season in leagueDetails.Data.Seasons)
            {
                try
                {
                    var seasonMatchDetails = await new GetSeasonMatchDetails().GetSeasonMatchDetailsAsync(user, token, season.Id);
                    seasons.Add(seasonMatchDetails);
                }
                catch(Exception ex)
                {

                }
            }

            //get all matches from seasons
            List<Datum> matches = new List<Datum>();
            foreach(var season in seasons.Where(x=>x.Data != null).ToList())
            {
                var finishedMatches = season.Data.FindAll(x => x.StatusName == "Finished");
                matches.AddRange(finishedMatches);
            }
            //order by date, descending
            matches = matches.OrderByDescending(x => x.Time.Timestamp).ToList();

            #endregion

            #region Calculate Match Features

            foreach (var match in matches)
            {

                #region Calculation

                var goalDiffRes = CalculateGoalDifference(matches, match);                                
                var winRateRes = CalculateWinRate(matches, match);                
                double formMomentumHome = CalculateFormMomentum(matches, match.Teams.Home.Id);                                
                double formMomentumAway = CalculateFormMomentum(matches, match.Teams.Away.Id);

                #endregion

                matchFeatures.Add(new MatchFeatures
                {
                    MatchId = match.Id,
                    HomeTeam = match.Teams.Home.Name,
                    AwayTeam = match.Teams.Away.Name,
                    Date = match.Time.Date,
                    GoalDifference = goalDiffRes.homePastAvg - goalDiffRes.awayPostAvg,
                    WinRateAway = winRateRes.winRateAway,
                    WinRateHome = winRateRes.winRateHome,
                    FormMomentumHome = formMomentumHome,
                    FormMomentumAway = formMomentumAway,
                    LeagueRankDifference = match.Standings.HomePosition - match.Standings.AwayPosition,
                    HomeGoals = !string.IsNullOrEmpty(match.Scores.HomeScore) ? Int32.Parse(match.Scores.HomeScore) : 0,
                    AwayGoals = !string.IsNullOrEmpty(match.Scores.AwayScore) ? Int32.Parse(match.Scores.AwayScore) : 0,
                });

            }

            #endregion

            return matchFeatures;
        }

        #region Calculate Methods

        private (double homePastAvg, double awayPostAvg) CalculateGoalDifference(List<Datum> matches, Datum match)
        {
            var pastHomeMatches = matches.Where(x =>
                                      x.Teams.Home.Id == match.Teams.Home.Id &&
                                      new DateTime(x.Time.Timestamp) < new DateTime(match.Time.Timestamp)).ToList();
            var scoredHomeGoals = pastHomeMatches.Select(x => { 
                                                                int parseHomeScore = 0; 
                                                                Int32.TryParse(x.Scores.HomeScore, out parseHomeScore); 
                                                                return parseHomeScore; 
                                                            }).Sum();  

            var pastAwayMatches = matches.Where(x =>
                                  x.Teams.Away.Id == match.Teams.Away.Id &&
                                  new DateTime(x.Time.Timestamp) < new DateTime(match.Time.Timestamp)).ToList();
            var scoredAwayGoals = pastAwayMatches.Select(x => { 
                                                                int parseAwayScore = 0; 
                                                                Int32.TryParse(x.Scores.AwayScore, out parseAwayScore); 
                                                                return parseAwayScore; 
                                                            }).Sum();

            double homePastAvg = pastHomeMatches.Count > 0 ? (double)scoredHomeGoals / (double)pastHomeMatches.Count : 0;
            double awayPastAvg = pastAwayMatches.Count > 0 ? (double)scoredAwayGoals / (double)pastAwayMatches.Count : 0;

            return (homePastAvg, awayPastAvg);
        }

        private (double winRateHome, double winRateAway) CalculateWinRate(List<Datum> matches, Datum match)
        {
            var winHomeMatches = matches.FindAll(x => { int parseHomeScore = 0; int parseAwayScore = 0; Int32.TryParse(x.Scores.HomeScore, out parseHomeScore); Int32.TryParse(x.Scores.AwayScore, out parseAwayScore); return x.Teams.Home.Id == match.Teams.Home.Id && parseHomeScore > parseAwayScore; });
            var totalHomeMatches = matches.FindAll(x => x.Teams.Home.Id == match.Teams.Home.Id);
            double winRateHome = ((double)winHomeMatches.Count / (double)totalHomeMatches.Count) * 100d;

            var winAwayMatches = matches.FindAll(x => { int parseHomeScore = 0; int parseAwayScore = 0; Int32.TryParse(x.Scores.HomeScore, out parseHomeScore); Int32.TryParse(x.Scores.AwayScore, out parseAwayScore); return x.Teams.Away.Id == match.Teams.Away.Id && parseHomeScore < parseAwayScore; });
            var totalAwayMatches = matches.FindAll(x => x.Teams.Away.Id == match.Teams.Away.Id);
            double winRateAway = ((double)winAwayMatches.Count / (double)totalAwayMatches.Count) * 100;

            return (winRateHome, winRateAway);
        }        

        private double CalculateFormMomentum(List<Datum> matches, int teamId)
        {
            var lastMatchesOfTeam = matches.Where(x =>
                                                          x.Teams.Home.Id == teamId ||
                                                          x.Teams.Away.Id == teamId).
                                                          Skip(0).Take(SoccerAPIConsts.FormMomentumMax).ToList();

            lastMatchesOfTeam = lastMatchesOfTeam.OrderBy(x=>x.Time.Timestamp).ToList();

            double sumOfPoints = 0;
            double sumOfWeights = 0;
            var listOfWeights = GetWeights();           
            for (var i = 0; i < lastMatchesOfTeam.Count; i++)
            {
                double weight = listOfWeights[i];
               
                sumOfWeights += weight;
                var isHomeTeam = lastMatchesOfTeam[i].Teams.Home.Id == teamId ? true : false;

                int parseHomeScore = 0; Int32.TryParse(lastMatchesOfTeam[i].Scores.HomeScore, out parseHomeScore);
                int parseAwayScore = 0; Int32.TryParse(lastMatchesOfTeam[i].Scores.AwayScore, out parseAwayScore);
                if (isHomeTeam)
                    sumOfPoints += weight *
                        (parseHomeScore > parseAwayScore ?
                        SoccerAPIConsts.Win :
                        (parseHomeScore == parseAwayScore ?
                        SoccerAPIConsts.Draw : SoccerAPIConsts.Lost));                    
                else                
                    sumOfPoints += weight *
                        (parseAwayScore > parseHomeScore ? 
                        SoccerAPIConsts.Win :
                        (parseAwayScore == parseHomeScore ? 
                        SoccerAPIConsts.Draw : SoccerAPIConsts.Lost));                
            }

            double formMomentum = sumOfPoints / sumOfWeights;
            return formMomentum;
        }

        #region Helper Methods

        public List<double> GetWeights()
        {
            List<double> toReturn = new List<double>();

            double weightStep = 1d / (double)SoccerAPIConsts.FormMomentumMax;
            for (int i = 0; i <= SoccerAPIConsts.FormMomentumMax; i++)
            {
                if (i == 0) continue;
                toReturn.Add(Math.Round(weightStep * i,1));
            }

            return toReturn;
        }

        #endregion

        #endregion

        #endregion
    }
}
