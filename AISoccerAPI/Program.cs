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

    //load settings
    //soccer api settings
    var user = configuration["SoccerAPI:user"];
    var token = configuration["SoccerAPI:token"];
    var leagueIds = configuration["SoccerAPI:soccerAPILeagueIds"];
    var predictLeagueIDs = configuration["SoccerAPI:predictLeagueIDs"];
    //app settings
    var folderPath = configuration["AppSettings:baseFolderPath"];
    var matchFeaturesCSVFileName = configuration["AppSettings:matchFeaturesCSVFileName"];
    var csvFilePath = configuration["AppSettings:csvFilePath"] + DateTime.Now.ToString("yyyyMMdd") + "_" + matchFeaturesCSVFileName;
    var predictoinCSVFileName = configuration["AppSettings:predictionCSVFileName"];

    #endregion

    #region Training Model

    //prepare data for training
    var trainData = Convert.ToBoolean(configuration["AppSettings:trainData"]);
    if (trainData)
        await new PrepareData().PrepareDataForTraining(leagueIds, user, token, csvFilePath, folderPath, DateTime.Now.ToString("yyyyMMdd") + "_" + matchFeaturesCSVFileName);

    #endregion

    #region Predictions

    //make predictions
    var predictData = Convert.ToBoolean(configuration["AppSettings:predictData"]);
    if (predictData)
    {        
        
        var predictLeagueIDsArray = predictLeagueIDs.Split(new char[1] { ',' });
        List<MatchPredictionResult> predictionResults = new List<MatchPredictionResult>();
        foreach(var predictLeagueID in predictLeagueIDsArray)        
            predictionResults.AddRange(await new FixtureData().GetFixturesPrediction(user, token, predictLeagueID, csvFilePath, folderPath));  
        
        new CSVSerialization().SaveMatchPredictionsToCsv(predictionResults, folderPath + DateTime.Now.ToString("yyyyMMdd") + "_" + predictoinCSVFileName);
    }

    #endregion

    #region New Sources

    //for new matches
    var client = new RestClient("https://v3.football.api-sports.io/leagues");
    var request = new RestRequest();
    request.AddHeader("x-rapidapi-key", "0761875f04ccc598b6815cc8780ac0f3");
    request.AddHeader("x-rapidapi-host", "v3.football.api-sports.io");
    RestResponse response = client.Execute(request);
    Console.WriteLine(response.Content);

    #endregion
}
catch (Exception ex)
{

}