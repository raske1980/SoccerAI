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
        public async Task PrepareDataForTraining(string leagueIds,
            string user,
            string token,
            string csvFilePath,
            string folderPath,
            string matchFeaturesCSVFileName)
        {
            //call api, transform data, save to csv
            var leagudeIdsArr = leagueIds.Split(new char[1] { ',' });
            List<MatchFeatures> matchFeatures = new List<MatchFeatures>();
            foreach (var leagueId in leagudeIdsArr)
            {
                var leagueDetails = await new GetLeagueDetail().GetSoccerLeagueAsync(user, token, leagueId);
                matchFeatures.AddRange(await new CalculateSoccerAPI().CalculateMatchFeatures(leagueDetails, user, token));
            }

            new CSVSerialization().SaveFeaturesToCsv(matchFeatures, folderPath, csvFilePath);

            new TrainModel().StartTrainModel(folderPath, matchFeaturesCSVFileName);
        }
    }
}
