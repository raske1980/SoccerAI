using AISoccerAPI.Calculation;
using AISoccerAPI.Data;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISoccerAPI.Train.ML
{
    public class TrainMLModel
    {
        public void StartTrainModel(AppConfig appConfig)
        {
            //csv file path
            string csvFilePath = appConfig.AppSettingsConfig.BaseFolderPath + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName;
            //load and prepare data
            var mlContext = new MLContext();           
            IDataView data = mlContext.Data.LoadFromTextFile<MatchFeatures>(csvFilePath, separatorChar: ',', hasHeader: true);            

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
            FileInfo fInfo = new FileInfo(csvFilePath);            
            new SaveLoadModel().SaveModel(fInfo.Directory.FullName, homeModel,awayModel, trainingData); 
        }        
    }
}
