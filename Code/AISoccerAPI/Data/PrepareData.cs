using AISoccerAPI.API.SoccerAPI.SoccerLeagueDetail;
using AISoccerAPI.Calculation.SoccerAPI;
using AISoccerAPI.Calculation;
using AISoccerAPI.ML;
using AISoccerAPI.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISoccerAPI.Data
{
    public class PrepareData
    {
        public async Task PrepareDataForTraining(AppConfig appConfig)
        {
            //call api, transform data, save to csv
            var leagudeIdsArr = appConfig.SoccerAPIConfig.SoccerAPILeagueIDs.Split(new char[1] { ',' });
            List<MatchFeatures> matchFeatures = new List<MatchFeatures>();
            foreach (var leagueId in leagudeIdsArr)
            {
                var leagueDetails = await new GetLeagueDetail().GetSoccerLeagueAsync(appConfig.SoccerAPIConfig.User, appConfig.SoccerAPIConfig.Token, leagueId);
                matchFeatures.AddRange(await new CalculateSoccerAPI().CalculateMatchFeatures(leagueDetails, appConfig.SoccerAPIConfig.User, appConfig.SoccerAPIConfig.Token));
            }

            new CSVSerialization().SaveFeaturesToCsv(matchFeatures,
                appConfig.SoccerAPIConfig.BaseFolderPath + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName);            
        }
    }
}
