using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using AISoccerAPI.Serialization;
using AISoccerAPI.API.SoccerAPI.SoccerRoundFixtures;
using AISoccerAPI.Data;
using AISoccerAPI.Train.TensorFlow;
using AISoccerAPI.JSON.Merge;
using AISoccerAPI.Train.ML;
using AISoccerAPI.API.FootballData;
using AISoccerAPI.API.FootballData.Data;
using Google.Protobuf.WellKnownTypes;
using System.Linq.Expressions;
using AISoccerAPI.Train;


try
{

    #region Load Configuration

    //get configuration
    var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                })
                .Build();
    var configuration = host.Services.GetService<IConfiguration>();
    var appConfig =  new AppConfig(configuration);
    

    #endregion

    #region Training Model
    
    if (appConfig.AppSettingsConfig.TrainData)
    {
        //merge API and JSON sources
        await new Merge().StartMergeAll(appConfig);

        //train models
        new Train().TrainModels(appConfig);
    }        

    #endregion

    #region Predictions

    //make predictions
    if (appConfig.AppSettingsConfig.PredictData)
    {        
        
        var predictLeagueIDsArray = appConfig.SoccerAPIConfig.PredictLeagueIDs.Split(new char[1] { ',' });
        List<MatchPredictionResult> predictionResults = new List<MatchPredictionResult>();
        foreach(var predictLeagueID in predictLeagueIDsArray)        
            predictionResults.AddRange(await new FixtureData().GetFixturesPrediction(appConfig, predictLeagueID));  
        
        new CSVSerialization().SaveMatchPredictionsToCsv(predictionResults, 
            appConfig.AppSettingsConfig.BaseFolderPath + DateTime.Now.ToString("yyyyMMdd") + "_" + appConfig.AppSettingsConfig.PredictionCSVFileName);
    }

    #endregion

    #region New Sources        

    #endregion

    Console.ReadLine();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    Console.WriteLine(ex.StackTrace);
}