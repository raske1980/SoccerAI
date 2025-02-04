using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISoccerAPI.Data
{


//    {
//    "AppSettings": {
//        "baseFolderPath": "C:\\projekti\\SoccerAI\\MLSoccer\\CSV\\",
//        "matchFeaturesCSVFileName": "MatchFeatures.csv",
//        "predictionCSVFileName": "Prediction.csv",
//        "trainData": "false",
//        "predictData": "false"
//    },
//    "SoccerAPI": {
//    "baseFolderPath": "C:\\projekti\\SoccerAI\\MLSoccer\\CSV\\SoccerAPI\\",
//        "user": "raske1980",
//        "token": "e80623ecb0540b16a0359477e614124c",
//        "soccerAPILeagueIds": "637,3104,3060,2878,719,721,594,764,583,574,1609,1580,1082,1005,580,581,765,592,1937,638",
//        "predictLeagueIDs": "583,574,580,581"
//    },
//    "FootballAPI": {
//    "baseFolderPath": "C:\\projekti\\SoccerAI\\MLSoccer\\CSV\\FootballAPI\\",
//        "apiURL": "v3.football.api-sports.io",
//        "key": "0761875f04ccc598b6815cc8780ac0f3"
//    }
//}

    public class AppConfig
    {
        public AppSettingsConfig AppSettingsConfig { get; set; }
        public SoccerAPIConfig SoccerAPIConfig { get; set; }

        public FootballAPIConfig FootballAPIConfig { get; set; }
        public AppConfig(IConfiguration configuration) 
        {
            this.AppSettingsConfig = new AppSettingsConfig(configuration);
            this.SoccerAPIConfig = new SoccerAPIConfig(configuration);
            this.FootballAPIConfig = new FootballAPIConfig(configuration);
        }
    }

    public class FootballAPIConfig
    {
        public string BaseFolderPath { get; set; }
        public string APIUrl { get; set; }
        public string Key { get; set; }

        public FootballAPIConfig(IConfiguration configuration)
        {
            this.APIUrl = configuration["FootballAPI:apiURL"];
            this.Key = configuration["FootballAPI:key"];
            this.BaseFolderPath = configuration["FootballAPI:baseFolderPath"];
        }
    }

    public class SoccerAPIConfig
    {
        public string BaseFolderPath { get; set; }
        public string User { get; set; }
        public string Token { get; set; }
        public string SoccerAPILeagueIDs { get; set; }
        public string PredictLeagueIDs { get; set; }

        public SoccerAPIConfig(IConfiguration configuration)
        {
            this.User = configuration["SoccerAPI:user"];
            this.Token = configuration["SoccerAPI:token"];
            this.SoccerAPILeagueIDs = configuration["SoccerAPI:soccerAPILeagueIds"];
            this.PredictLeagueIDs = configuration["SoccerAPI:predictLeagueIDs"];
            this.BaseFolderPath = configuration["SoccerAPI:baseFolderPath"];            
        }
    }

    public class AppSettingsConfig
    {
        public string BaseFolderPath { get; set; }
        public string MatchFeaturesCSVFileName { get; set; }
        public string PredictionCSVFileName { get; set; }
        public bool TrainData { get; set; }
        public bool PredictData { get; set; }

        public AppSettingsConfig(IConfiguration configuration)
        {
            this.BaseFolderPath = configuration["AppSettings:baseFolderPath"];
            this.MatchFeaturesCSVFileName = DateTime.Now.ToString("yyyyMMdd") + "_" + configuration["AppSettings:matchFeaturesCSVFileName"];           
            this.PredictionCSVFileName = configuration["AppSettings:predictionCSVFileName"];
            this.TrainData = Convert.ToBoolean(configuration["AppSettings:trainData"]);
            this.PredictData = Convert.ToBoolean(configuration["AppSettings:predictData"]);
        }
    }
}
