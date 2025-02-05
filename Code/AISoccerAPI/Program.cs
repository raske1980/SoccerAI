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
        await new PrepareData().PrepareDataForTraining(
            appConfig.SoccerAPIConfig.SoccerAPILeagueIDs, 
            appConfig.SoccerAPIConfig.User, 
            appConfig.SoccerAPIConfig.Token, 
            appConfig.SoccerAPIConfig.BaseFolderPath + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName);

    #endregion

    #region Predictions

    //make predictions
    if (appConfig.AppSettingsConfig.TrainData)
    {        
        
        var predictLeagueIDsArray = appConfig.SoccerAPIConfig.PredictLeagueIDs.Split(new char[1] { ',' });
        List<MatchPredictionResult> predictionResults = new List<MatchPredictionResult>();
        foreach(var predictLeagueID in predictLeagueIDsArray)        
            predictionResults.AddRange(await new FixtureData().GetFixturesPrediction(
                appConfig.SoccerAPIConfig.User, 
                appConfig.SoccerAPIConfig.Token, 
                predictLeagueID, 
                appConfig.SoccerAPIConfig.BaseFolderPath + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName));  
        
        new CSVSerialization().SaveMatchPredictionsToCsv(predictionResults, 
            appConfig.SoccerAPIConfig.BaseFolderPath + DateTime.Now.ToString("yyyyMMdd") + "_" + appConfig.AppSettingsConfig.PredictionCSVFileName);
    }

    #endregion

    #region New Sources


    //get leagues by country
    //var client2 = new RestClient("https://v3.football.api-sports.io/leagues?code=FR");
    //var request2 = new RestRequest();
    //request2.AddHeader("x-rapidapi-key", footballAPIKey);
    //request2.AddHeader("x-rapidapi-host", footballAPIUrl);
    //RestResponse response2 = client2.Execute(request2);   

    ////get season info by id
    //var client1 = new RestClient("https://v3.football.api-sports.io/leagues?season=2021");
    //var request1 = new RestRequest();
    //request1.AddHeader("x-rapidapi-key", footballAPIKey);
    //request1.AddHeader("x-rapidapi-host", footballAPIUrl);
    //RestResponse response1 = client1.Execute(request1);    

    ////get match info by season id and match id, https://v3.football.api-sports.io/leagues?season=2019&id=39");
    //var client3 = new RestClient("https://v3.football.api-sports.io/leagues?season=2021&id=61");
    //var request3 = new RestRequest();
    //request3.AddHeader("x-rapidapi-key", footballAPIKey);
    //request3.AddHeader("x-rapidapi-host", footballAPIUrl);
    //RestResponse response3 = client3.Execute(request3);    

    ////get fixtures by season id, https://v3.football.api-sports.io/fixtures?league=39&season=2023
    //var client4 = new RestClient("https://v3.football.api-sports.io/fixtures?league=39&season=2023");
    //var request4 = new RestRequest();
    //request4.AddHeader("x-rapidapi-key", footballAPIKey);
    //request4.AddHeader("x-rapidapi-host", footballAPIUrl);
    //RestResponse response4 = client4.Execute(request4);    
    //var fixtureResponse = JsonConvert.DeserializeObject<FootbalAPIFixtureResponse>(response4.Content);
    
    //to calculate match features for Football API, to make union of match features from Soccer API
    //and Football API and to save result of that union to the root folder, and use that saved union
    //for training models which will be saved also in root folder, and those models will be used for predictions
    //which will be saved also in root folder

    #endregion
}
catch (Exception ex)
{

}