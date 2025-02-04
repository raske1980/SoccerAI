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

    //for new matches, Football API, get all leagues
    //var client = new RestClient("https://v3.football.api-sports.io/leagues");
    //var request = new RestRequest();
    //request.AddHeader("x-rapidapi-key", footballAPIKey);
    //request.AddHeader("x-rapidapi-host", footballAPIUrl);
    //RestResponse response = client.Execute(request);
    //var apiLeagueDetailResponses = JsonConvert.DeserializeObject<FootballAPILeaguesResponse>(response.Content);   
    //var apiFootballLeagues = apiLeagueDetailResponses.Response.FindAll(x => x.League.Type == "League").ToList();

    ////create list of exclusions (leagues that we are getting through Soccer API)
    //var footballAPIMatchExclusions = new List<int>();
    //var soccerApiExclusions = new Exclusions().GetSoccerApiLeaaguesByCountry();
    //foreach(var soccerAPIExclusion in soccerApiExclusions)
    //{
    //    var matchedLeague = apiLeagueDetailResponses.Response.FirstOrDefault(x => 
    //                                                                         x.League.Name.ToLower().Trim() == soccerAPIExclusion.league.ToLower().Trim() &&
    //                                                                         x.Country.Name.ToLower().Trim() == soccerAPIExclusion.country.ToLower().Trim());
    //    if (matchedLeague == null)                    
    //        matchedLeague = apiLeagueDetailResponses.Response.FirstOrDefault(x =>
    //                                                                         x.League.Name.ToLower().Trim() == soccerAPIExclusion.footballAPIMappingName.ToLower().Trim() &&
    //                                                                         x.Country.Name.ToLower().Trim() == soccerAPIExclusion.country.ToLower().Trim());        

    //    if (matchedLeague != null)
    //        footballAPIMatchExclusions.Add(matchedLeague.League.Id);
    //}

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
    
    //work flow will be like this, first call I am going to get all the leagues,
    //I must create exclusion list of leagues that I already have,
    //I will iterate through league's seasons by id and get fixtures for that season id,
    //I am going to create list of matches by league by iterating through all league's seasons,
    //and I am going to use that list to save it as MatchFeatures.csv file by using season id inside league folder
    //and later I am going to concanate all csvs with SoccerApi csv and train the model at the end,
    //for beginning I am going to create up to 90 csvs per day because of the free plan limitations (100 requests per day)
    //so there is about 700+ leagues and I can get for free 3 seasons per league (2021,2022,2023) so it will take about 3.5 weeks to get all the data,
    //but I will train model at the end of the day with newly aquired data,
    //so I will need to create mechanism for csv concatenation after I create mechanism for Football API csv serialiazation as MatchFeature objects
    //csv serialization must include yyyyMMdd info so I will work only with newest csv files like I created for model save/load

    #endregion
}
catch (Exception ex)
{

}