using AISoccerAPI.API.SoccerAPI.SoccerLeagueDetail;
using AISoccerAPI.API.SoccerAPI.SoccerLeaguesDetail;
using AISoccerAPI.API.SoccerAPI.SoccerLeagueStandings;
using AISoccerAPI.API.SoccerAPI.SoccerSeasonMathesDetail;
using AISoccerAPI.Calculation;
using AISoccerAPI.Calculation.SoccerAPI;
using AISoccerAPI.Consts;
using AISoccerAPI.Data;
using AISoccerAPI.Train.ML;
using AISoccerAPI.Serialization;
using AISoccerAPI.Train.TensorFlow;
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
using Tensorflow;
using Tensorflow.Keras.Engine;
using Tensorflow.NumPy;

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
            //GetTFVariables(pastMatches, tfModel);


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

                //add prediction to the list with its duplicate that will serve as actual match data where we are going to populate with actual results                
                //tf predictions
                //newMatch.HomeTeam = homeTeam;
                //newMatch.AwayTeam = awayTeam;
                //var predictionTF = new PredictTF().Predict(newMatch, tfModel);
                //predictions.Add(new MatchPredictionResult
                //{
                //    Category = MatchCategory.Prediction,
                //    Source = PredictionSource.TF,
                //    HomeTeam = homeTeam,
                //    AwayTeam = awayTeam,
                //    HomeTeamGoalRange = string.Empty,// $"{Math.Floor(predictionTF.homeGoalsPrediction - tfVariables.mae)} - {Math.Ceiling(predictionTF.homeGoalsPrediction + tfVariables.mae)}",
                //    AwayTeamGoalRange = string.Empty,// $"{Math.Floor(predictionTF.awayGoalsPrediction - tfVariables.mae)} - {Math.Ceiling(predictionTF.awayGoalsPrediction + tfVariables.mae)}",
                //    HomeTeamGoals = Math.Round(predictionTF.homeGoalsPrediction, 1),
                //    AwayTeamGoals = Math.Round(predictionTF.awayGoalsPrediction, 1),
                //    TotalGoals = Math.Round(Math.Round(predictionTF.homeGoalsPrediction, 1) + Math.Round(predictionTF.awayGoalsPrediction, 1), 1),
                //    DatePlayed = currentRoundFixture.Time.Date
                //});

                //ml predictions
                predictions.Add(new MatchPredictionResult
                {
                    Category = MatchCategory.Prediction,
                    Source = PredictionSource.ML,
                    HomeTeam = homeTeam,
                    AwayTeam = awayTeam,
                    HomeTeamGoals = Math.Round(predictedHomeGoals, 1),
                    AwayTeamGoals = Math.Round(predictedAwayGoals, 1),
                    HomeTeamGoalRange = string.Empty,
                    AwayTeamGoalRange = string.Empty,
                    TotalGoals = Math.Round(Math.Round(predictedHomeGoals, 1) + Math.Round(predictedAwayGoals, 1), 1),
                    DatePlayed = currentRoundFixture.Time.Date
                });

                //actual result
                predictions.Add(new MatchPredictionResult
                {
                    Category = MatchCategory.Actual,
                    Source = string.Empty,
                    HomeTeam = homeTeam,
                    AwayTeam = awayTeam,
                    HomeTeamGoals = 0,
                    AwayTeamGoals = 0,
                    HomeTeamGoalRange = string.Empty,
                    AwayTeamGoalRange = string.Empty,
                    TotalGoals = 0,
                    DatePlayed = currentRoundFixture.Time.Date
                });

                Console.WriteLine();
            }
            Console.WriteLine();
            Console.WriteLine();

            return predictions;
        }

        #region Private Methods

        private static float[,] ConvertTo2DArray(List<float[]> jaggedArray)
        {
            if (jaggedArray == null || jaggedArray.Count == 0)
                throw new ArgumentException("Error: Input data is empty!");

            int rows = jaggedArray.Count;
            int cols = jaggedArray[0].Length;
            float[,] result = new float[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                if (jaggedArray[i].Length != cols)
                    throw new InvalidOperationException("Error: Inconsistent row lengths in input data.");

                for (int j = 0; j < cols; j++)
                    result[i, j] = jaggedArray[i][j];
            }
            return result;
        }

        private float[][] NormalizeData(float[][] data)
        {
            int numFeatures = data[0].Length;
            float[] min = new float[numFeatures];
            float[] max = new float[numFeatures];

            for (int j = 0; j < numFeatures; j++)
            {
                min[j] = data.Min(row => row[j]);
                max[j] = data.Max(row => row[j]);
            }

            return data.Select(row => row.Select((value, j) => (value - min[j]) / (max[j] - min[j])).ToArray()).ToArray();
        }

        private (NDArray, NDArray, NDArray, NDArray) PrepareData(List<MatchFeatures> matches)
        {
            if (matches == null || matches.Count == 0)
                throw new Exception("Error: No match data available for training.");

            var inputData = matches.Select(m => new float[]
            {
                (float)m.GoalDifference,
                (float)m.WinRateHome,
                (float)m.WinRateAway,
                (float)m.FormMomentumHome,
                (float)m.FormMomentumAway,
                (float)m.LeagueRankDifference
            }).ToArray();

            var outputData = matches.Select(m => new float[]
            {
                (float)m.HomeGoals,
                (float)m.AwayGoals
            }).ToArray();

            // **Normalize Data Using Min-Max Scaling**
            inputData = NormalizeData(inputData);

            int splitIndex = (int)(matches.Count * 0.8);

            float[,] trainXArray = ConvertTo2DArray(inputData.Take(splitIndex).ToList());
            float[,] trainYArray = ConvertTo2DArray(outputData.Take(splitIndex).ToList());
            float[,] testXArray = ConvertTo2DArray(inputData.Skip(splitIndex).ToList());
            float[,] testYArray = ConvertTo2DArray(outputData.Skip(splitIndex).ToList());

            return (
                np.array(trainXArray, dtype: np.float32),
                np.array(trainYArray, dtype: np.float32),
                np.array(testXArray, dtype: np.float32),
                np.array(testYArray, dtype: np.float32)
            );
        }

        //public void GetTFVariables(List<MatchFeatures> allMatchFeatures, IModel tfModel)
        //{
        //    Console.WriteLine($"🔹 Starting GetTFVariables...");

        //    // ✅ Ensure the model is loaded correctly
        //    if (tfModel == null)
        //    {
        //        Console.WriteLine("❌ Model is NULL! Ensure it is loaded properly.");
        //        return;
        //    }

        //    Console.WriteLine($"✅ Model Loaded Successfully!");

        //    // ✅ Prepare test data
        //    (NDArray testX, NDArray testY,_,_) = PrepareData(allMatchFeatures);

        //    // ✅ Check if test data is valid
        //    if (testX == null || testY == null)
        //    {
        //        Console.WriteLine("❌ testX or testY is NULL!");
        //        return;
        //    }

        //    Console.WriteLine($"✅ testX Shape: {testX.shape}");
        //    Console.WriteLine($"✅ testY Shape: {testY.shape}");

        //    // ✅ Check for NaN / Infinity in testX & testY
        //    if (CheckForNaNOrInfinity(testX))
        //    {
        //        Console.WriteLine("⚠️ testX contains NaN or Infinite values!");
        //        return;
        //    }

        //    if (CheckForNaNOrInfinity(testY))
        //    {
        //        Console.WriteLine("⚠️ testY contains NaN or Infinite values!");
        //        return;
        //    }

        //    Console.WriteLine($"✅ testX and testY are clean (no NaN/Inf detected).");

        //    // ✅ Display Model Input Shape
        //    Console.WriteLine($"🔹 Model First Layer: {tfModel.Layers.FirstOrDefault()?.ToString()}");

        //    // ✅ Ensure testX shape matches model input shape
        //    var modelInputShape = tfModel.Layers.FirstOrDefault()?.OutputShape;
        //    if (modelInputShape == null)
        //    {
        //        Console.WriteLine("⚠️ Warning: Could not retrieve model input shape!");
        //    }
        //    else
        //    {
        //        Console.WriteLine($"✅ Model Expected Input Shape: {modelInputShape}");
        //    }

        //    Console.WriteLine($"✅ testX Shape Before Evaluation: {testX.shape}");

        //    // ✅ Run Model Evaluation
        //    Console.WriteLine($"🔹 Running Model Evaluation...");

        //    try
        //    {
        //        Dictionary<string, float> evaluation = tfModel.evaluate(testX, testY, batch_size: 256, return_dict: true);

        //        if (evaluation == null)
        //        {
        //            Console.WriteLine("❌ Evaluation returned NULL!");
        //            return;
        //        }

        //        Console.WriteLine($"✅ Model Evaluation Completed!");

        //        foreach (var kvp in evaluation)
        //        {
        //            Console.WriteLine($"🔹 {kvp.Key}: {kvp.Value}");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"❌ Model Evaluation Failed: {ex.Message}");
        //        return;
        //    }

        //    // ✅ If evaluate() fails, try predict()
        //    try
        //    {
        //        Console.WriteLine($"🔹 Running Model Predictions...");

        //        Tensors rawPredictions = tfModel.predict(testX);
        //        NDArray predictions = rawPredictions[0].numpy();

        //        Console.WriteLine($"✅ Prediction Successful! Predictions Shape: {predictions.shape}");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"❌ Model Prediction Failed: {ex.Message}");
        //    }
        //}


        //// 🛠 Helper Method to Check for NaN/Inf Manually
        //private bool CheckForNaNOrInfinity(NDArray array)
        //{
        //    float[] flatArray = array.ToArray<float>();
        //    return flatArray.Any(float.IsNaN) || flatArray.Any(float.IsInfinity);
        //}

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

