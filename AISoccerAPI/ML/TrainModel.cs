using AISoccerAPI.Calculation;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISoccerAPI.ML
{
    public class TrainModel
    {
        public void StartTrainModel(string path, string fileName)
        {
            //load and prepare data
            var mlContext = new MLContext();
            string dataPath = path + fileName;
            IDataView data = mlContext.Data.LoadFromTextFile<MatchFeatures>(dataPath, separatorChar: ',', hasHeader: true);

            // Split data
            var split = mlContext.Data.TrainTestSplit(data, testFraction: 0.2);
            var trainingData = split.TrainSet;
            var testingData = split.TestSet;

            //define pipelines           
            // HomeGoals pipeline
            var homePipeline = mlContext.Transforms.CopyColumns("Label", "HomeGoals")
                        .Append(mlContext.Transforms.Concatenate("Features", "GoalDifference", "WinRateHome", "FormMomentumHome", "LeagueRankDifference"))
                        .Append(mlContext.Transforms.Conversion.ConvertType("Features", outputKind: DataKind.Single)) // Convert features to Single
                        .Append(mlContext.Regression.Trainers.FastTree());

            // AwayGoals pipeline
            var awayPipeline = mlContext.Transforms.CopyColumns("Label", "AwayGoals")
                .Append(mlContext.Transforms.Concatenate("Features", "GoalDifference", "WinRateAway", "FormMomentumAway", "LeagueRankDifference"))
                .Append(mlContext.Transforms.Conversion.ConvertType("Features", outputKind: DataKind.Single)) // Convert features to Single
                .Append(mlContext.Regression.Trainers.FastTree());


            //train model
            // Train the HomeGoals model
            var homeModel = homePipeline.Fit(trainingData);

            // Train the AwayGoals model
            var awayModel = awayPipeline.Fit(trainingData);

            //save model to the disk
            new SaveLoadModel().SaveModel(path, homeModel,awayModel, trainingData); 
        }

        
    }
}
