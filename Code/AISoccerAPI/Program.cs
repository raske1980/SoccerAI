using AISoccerAPI.API.SoccerAPI.SoccerLeagueDetail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using AISoccerAPI.Calculation.SoccerAPI;
using AISoccerAPI.Serialization;
using AISoccerAPI.ML;
using AISoccerAPI.Calculation;
using AISoccerAPI.API.SoccerAPI.SoccerRoundFixtures;
using AISoccerAPI.Data;
using RestSharp;
using Newtonsoft.Json;
using AISoccerAPI.Consts;
using AISoccerAPI.JSON.OpenData;
using AISoccerAPI.JSON.FootballJSON;
using AISoccerAPI.JSON.Merge;


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
        //Soccer API
        await new PrepareData().PrepareDataForTraining(appConfig);

        //Football API
        //await new AISoccerAPI.API.FootballAPI.PrepareData().GetAPIData(appConfig);
        await new AISoccerAPI.API.FootballAPI.PrepareData().GetAPIDataForDates(appConfig);
         
        //merge features from different sources
        new MergeMultipleSources().MergeFeatures(appConfig);

        //train data based on data from all sources
        new TrainModel().StartTrainModel(appConfig.AppSettingsConfig.BaseFolderPath + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName);
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

    //var openData =  new OpenDataExtract().OpenDataExtractPrepareData(appConfig);
    //var footballJSON = new FootballJSONExtract().PrepareData(appConfig);
    //var mergedFeatures = new Merge().MergeAll(openData, footballJSON, appConfig);
    //to obtain matches from football json, then to merge  them (remove duplicates) when
    //join match feature list is made for both json sources, then merged features
    //write to JSON folder and then it will be merged with others api features (also check for duplicates so JSON matches are not saved if
    //they already exist in API features list)
    
    #endregion
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    Console.WriteLine(ex.StackTrace);
}