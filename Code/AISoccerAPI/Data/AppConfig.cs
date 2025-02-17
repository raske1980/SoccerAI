using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISoccerAPI.Data
{
    public class AppConfig
    {
        public AppSettingsConfig AppSettingsConfig { get; set; }
        public SoccerAPIConfig SoccerAPIConfig { get; set; }

        public FootballAPIConfig FootballAPIConfig { get; set; }

        public OpenDataConfig OpenDataConfig { get; set; }

        public FootballJSONConfig FootballJSONConfig { get; set; }

        public AppConfig(IConfiguration configuration) 
        {
            this.AppSettingsConfig = new AppSettingsConfig(configuration);
            this.SoccerAPIConfig = new SoccerAPIConfig(configuration);
            this.FootballAPIConfig = new FootballAPIConfig(configuration);
            this.OpenDataConfig = new OpenDataConfig(configuration);
            this.FootballJSONConfig = new FootballJSONConfig(configuration);
        }
    }

    public class FootballAPIConfig
    {
        public string BaseFolderPath { get; set; }
        public string APIUrl { get; set; }
        public string Key { get; set; }
        public string ObtaindLeagueIDsFileName { get; set; }

        public FootballAPIConfig(IConfiguration configuration)
        {
            this.APIUrl = configuration["FootballAPI:apiURL"];
            this.Key = configuration["FootballAPI:key"];
            this.BaseFolderPath = configuration["FootballAPI:baseFolderPath"];
            this.ObtaindLeagueIDsFileName = configuration["FootballAPI:obtaindLeagueIDsFileName"];
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

    public class OpenDataConfig
    {
        public string BaseDataFolderPath { get; set; }
        public string BaseFolderPath { get; set; }

        public OpenDataConfig(IConfiguration configuration)
        {
            this.BaseDataFolderPath = configuration["OpenData:baseDataFolderPath"];
            this.BaseFolderPath = configuration["OpenData:baseFolderPath"];
        }
    }

    public class FootballJSONConfig
    {
        public string BaseDataFolderPath { get; set; }
        public string BaseFolderPath { get; set; }

        public FootballJSONConfig(IConfiguration configuration)
        {
            this.BaseDataFolderPath = configuration["FootballJSON:baseDataFolderPath"];
            this.BaseFolderPath = configuration["FootballJSON:baseFolderPath"];
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
            this.MatchFeaturesCSVFileName = configuration["AppSettings:matchFeaturesCSVFileName"];           
            this.PredictionCSVFileName = configuration["AppSettings:predictionCSVFileName"];
            this.TrainData = Convert.ToBoolean(configuration["AppSettings:trainData"]);
            this.PredictData = Convert.ToBoolean(configuration["AppSettings:predictData"]);
        }
    }
}
