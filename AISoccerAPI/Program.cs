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
    //football api
    var footballAPIUrl = configuration["FootballAPI:apiURL"];
    var footballAPIKey = configuration["FootballAPI:key"];

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

    //for new matches, Football API
    var client = new RestClient("https://v3.football.api-sports.io/leagues");
    var request = new RestRequest();
    request.AddHeader("x-rapidapi-key", footballAPIKey);
    request.AddHeader("x-rapidapi-host", footballAPIUrl);
    RestResponse response = client.Execute(request);
    var apiLeagueDetailResponses = JsonConvert.DeserializeObject<FootballAPILeaguesResponse>(response.Content);
    var apiFootballLeagueIds = apiLeagueDetailResponses.Response.FindAll(x=>x.League.Type == "League").ToList().Select(x=>x.League.Id).ToList();    

    #endregion
}
catch (Exception ex)
{

}