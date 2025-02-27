﻿using AISoccerAPI.API.SoccerAPI.SoccerLeagueDetail;
using AISoccerAPI.API.SoccerAPI.SoccerLeaguesDetail;
using AISoccerAPI.API.SoccerAPI.SoccerLeagueStandings;
using AISoccerAPI.API.SoccerAPI.SoccerSeasonMathesDetail;
using AISoccerAPI.Calculation;
using AISoccerAPI.Calculation.SoccerAPI;
using AISoccerAPI.Consts;
using AISoccerAPI.Data;
using AISoccerAPI.ML;
using AISoccerAPI.TensorFlow;
using CsvHelper;
using Microsoft.ML;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AISoccerAPI.API.SoccerAPI.SoccerRoundFixtures
{
    public class FixtureData
    {

        #region Constructors

        public FixtureData()
        {

        }

        #endregion

        #region Methods

        public async Task<List<MatchPredictionResult>> GetFixturesPrediction(AppConfig appConfig, string leagueId)
        {
            //get fixtures from the API
            var soccerLeague = await new GetLeagueDetail().GetSoccerLeagueAsync(appConfig.SoccerAPIConfig.User,
                appConfig.SoccerAPIConfig.Token, 
                leagueId);

            var currentRoundId = soccerLeague.Data.CurrentRoundId;
            var currentSeasonId = soccerLeague.Data.CurrentSeasonId;

            var seasonMatchDetails = await new GetSeasonMatchDetails().GetSeasonMatchDetailsAsync(appConfig.SoccerAPIConfig.User, 
                appConfig.SoccerAPIConfig.Token, 
                soccerLeague.Data.CurrentSeasonId);
            var seasonStandingsDetails = await new SoccerLeagueStanding().GetStandingAsync(appConfig.SoccerAPIConfig.User, 
                appConfig.SoccerAPIConfig.Token, 
                currentSeasonId);            
            //get all games that are going to be played in next 7 days
            var currentRoundFixtures = seasonMatchDetails.Data.FindAll(x => 
                                                                            Convert.ToDateTime(x.Time.Datetime) > DateTime.UtcNow && 
                                                                            Convert.ToDateTime(x.Time.Datetime) < DateTime.UtcNow.AddDays(7));

            //load past data from the excel
            var pastMatches = new List<MatchFeatures>();
            using (var reader = new StreamReader(appConfig.AppSettingsConfig.BaseFolderPath + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                pastMatches = csv.GetRecords<MatchFeatures>().ToList();               
            }

            //load models
            var models = new SaveLoadModel().LoadModels(
                new FileInfo(appConfig.AppSettingsConfig.BaseFolderPath + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName).Directory.FullName);

            //var tfModel = new SaveLoadTFModel().LoadModel(appConfig);
            
            List<MatchPredictionResult> predictions = new List<MatchPredictionResult>();

            //getting predictions objects
            foreach(var currentRoundFixture in currentRoundFixtures)
            {
                var homeTeam = currentRoundFixture.Teams.Home.Name;
                var homeTeamId = currentRoundFixture.Teams.Home.Id;
                var awayTeam = currentRoundFixture.Teams.Away.Name;
                var awayTeamId = currentRoundFixture.Teams.Away.Id;
                var homeStatistics = pastMatches.FindAll(x=>x.HomeTeam == homeTeam).ToList();
                var awayStatistics = pastMatches.FindAll(x=>x.AwayTeam == awayTeam).ToList();

                float averageHomeGoals = homeStatistics.Sum(x => x.HomeGoals) / (float)homeStatistics.Count;
                float averageAwayGoals = awayStatistics.Sum(x => x.AwayGoals) / (float)awayStatistics.Count;
                float goalDifference = averageHomeGoals - averageAwayGoals;

                float homeWins = ((float)homeStatistics.Where(x => x.HomeGoals > x.AwayGoals).ToList().Count / (float)homeStatistics.Count) * 100f;
                float awayWins = ((float)awayStatistics.Where(x => x.HomeGoals < x.AwayGoals).ToList().Count / (float)awayStatistics.Count) * 100f;

                float homeMomentum = CalculateFormMomentum(pastMatches, homeTeam);
                float awayMomentum = CalculateFormMomentum(pastMatches, awayTeam);

                var homePosition = seasonStandingsDetails.data.standings.FirstOrDefault(x => x.team_id == homeTeamId).overall.position;
                var awayPosition = seasonStandingsDetails.data.standings.FirstOrDefault(x => x.team_id == awayTeamId).overall.position;

                var newMatch = new MatchFeatures
                {
                    GoalDifference = goalDifference,      
                    WinRateHome = homeWins,          
                    WinRateAway = awayWins,          
                    FormMomentumHome = homeMomentum,    
                    FormMomentumAway = awayMomentum,    
                    LeagueRankDifference = homePosition != null && awayPosition != null ? (float)homePosition - (float)awayPosition : 0f
                };

                // Prediction engine for HomeGoals
                var mlContext = new MLContext();
                var homePredictionEngine = mlContext.Model.CreatePredictionEngine<MatchFeatures, MatchPrediction>(models.loadedHomeModel);
                float predictedHomeGoals = homePredictionEngine.Predict(newMatch).PredictedGoals;

                // Prediction engine for AwayGoals
                var awayPredictionEngine = mlContext.Model.CreatePredictionEngine<MatchFeatures, MatchPrediction>(models.loadedAwayModel);
                float predictedAwayGoals = awayPredictionEngine.Predict(newMatch).PredictedGoals;

                // Display predictions
                Console.WriteLine($"Predicted Home Goals For {homeTeam}: {Math.Round(predictedHomeGoals,1)}");
                Console.WriteLine($"Predicted Away Goals For {awayTeam}: {Math.Round(predictedAwayGoals,1)}");
                Console.WriteLine($"Predicted Total Goals For {homeTeam} - {awayTeam}: {Math.Round(Math.Round(predictedHomeGoals,1) + Math.Round(predictedAwayGoals,1),1)}");
                Console.WriteLine();
                //add prediction to the list with its duplicate that will serve as actual match data where we are going to populate with actual results
                predictions.Add(new MatchPredictionResult { 
                    Category = MatchCategory.Prediction,
                    HomeTeam = homeTeam,
                    AwayTeam = awayTeam,
                    HomeTeamGoals = Math.Round(predictedHomeGoals,1),
                    AwayTeamGoals = Math.Round(predictedAwayGoals,1),
                    TotalGoals = Math.Round(Math.Round(predictedHomeGoals,1) + Math.Round(predictedAwayGoals,1),1),     
                    DatePlayed = currentRoundFixture.Time.Date
                });

                predictions.Add(new MatchPredictionResult
                {
                    Category = MatchCategory.Actual,
                    HomeTeam = homeTeam,
                    AwayTeam = awayTeam,
                    HomeTeamGoals = 0,
                    AwayTeamGoals = 0,
                    TotalGoals = 0,
                    DatePlayed = currentRoundFixture.Time.Date
                });

                //tf predictions
                //newMatch.HomeTeam = homeTeam;
                //newMatch.AwayTeam = awayTeam;
                //new PredictTF().Predict(newMatch, tfModel);
            }
            Console.WriteLine();
            Console.WriteLine();

            return predictions;
        }

        #region Private Methods

        private float CalculateFormMomentum(List<MatchFeatures> matches, string team)
        {
            var lastMatchesOfTeam = matches.OrderByDescending(x=>x.Date).Where(x =>
                                                          x.HomeTeam == team ||
                                                          x.AwayTeam == team).
                                                          Skip(0).Take(APIConsts.FormMomentumMax).ToList();            

            float sumOfPoints = 0;
            float sumOfWeights = 0;
            var listOfWeights = CalculateSoccerAPI.GetWeights();
            for (var i = 0; i < lastMatchesOfTeam.Count; i++)
            {
                float weight = (float)listOfWeights[i];

                sumOfWeights += weight;
                var isHomeTeam = lastMatchesOfTeam[i].HomeTeam == team ? true : false;
                
                if (isHomeTeam)
                    sumOfPoints += (float)weight *
                        ((lastMatchesOfTeam[i].HomeGoals > lastMatchesOfTeam[i].AwayGoals) ?
                        APIConsts.Win :
                        (lastMatchesOfTeam[i].HomeGoals == lastMatchesOfTeam[i].AwayGoals) ?
                        APIConsts.Draw : APIConsts.Lost);
                else
                    sumOfPoints += (float)weight *
                        (lastMatchesOfTeam[i].AwayGoals > lastMatchesOfTeam[i].HomeGoals ?
                        APIConsts.Win :
                        (lastMatchesOfTeam[i].AwayGoals == lastMatchesOfTeam[i].HomeGoals) ?
                        APIConsts.Draw : APIConsts.Lost);
            }

            float formMomentum = sumOfPoints / sumOfWeights;
            return formMomentum;
        }

        #endregion

        #endregion

    }
}

