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

        #region Properties

        public AppSettingsConfig AppSettingsConfig { get; set; }
        public SoccerAPIConfig SoccerAPIConfig { get; set; }
        public FootballAPIConfig FootballAPIConfig { get; set; }
        public OpenDataConfig OpenDataConfig { get; set; }
        public FootballJSONConfig FootballJSONConfig { get; set; }
        public FootballDataConfig FootballDataConfig { get; set; }
        public TenserFlowConfig TenserFlowConfig { get; set; }

        #endregion

        #region Constructors

        public AppConfig(IConfiguration configuration) 
        {
            this.AppSettingsConfig = new AppSettingsConfig(configuration);
            this.SoccerAPIConfig = new SoccerAPIConfig(configuration);
            this.FootballAPIConfig = new FootballAPIConfig(configuration);
            this.OpenDataConfig = new OpenDataConfig(configuration);
            this.FootballJSONConfig = new FootballJSONConfig(configuration);
            this.FootballDataConfig = new FootballDataConfig(configuration);
            this.TenserFlowConfig = new TenserFlowConfig(configuration);
        }

        #endregion

    }

    public class FootballAPIConfig
    {

        #region Properties

        public string BaseFolderPath { get; set; }
        public string APIUrl { get; set; }
        public string Key { get; set; }
        public string ObtaindLeagueIDsFileName { get; set; }

        #endregion

        #region Constructors

        public FootballAPIConfig(IConfiguration configuration)
        {
            this.APIUrl = configuration["FootballAPI:apiURL"];
            this.Key = configuration["FootballAPI:key"];
            this.BaseFolderPath = configuration["FootballAPI:baseFolderPath"];
            this.ObtaindLeagueIDsFileName = configuration["FootballAPI:obtaindLeagueIDsFileName"];
        }

        #endregion

    }

    public class SoccerAPIConfig
    {

        #region Properties

        public string BaseFolderPath { get; set; }
        public string User { get; set; }
        public string Token { get; set; }
        public string SoccerAPILeagueIDs { get; set; }
        public string PredictLeagueIDs { get; set; }

        #endregion

        #region Constructors

        public SoccerAPIConfig(IConfiguration configuration)
        {
            this.User = configuration["SoccerAPI:user"];
            this.Token = configuration["SoccerAPI:token"];
            this.SoccerAPILeagueIDs = configuration["SoccerAPI:soccerAPILeagueIds"];
            this.PredictLeagueIDs = configuration["SoccerAPI:predictLeagueIDs"];
            this.BaseFolderPath = configuration["SoccerAPI:baseFolderPath"];            
        }

        #endregion
    }

    public class OpenDataConfig
    {

        #region Properties

        public string BaseDataFolderPath { get; set; }
        public string BaseFolderPath { get; set; }

        #endregion

        #region Constructors

        public OpenDataConfig(IConfiguration configuration)
        {
            this.BaseDataFolderPath = configuration["OpenData:baseDataFolderPath"];
            this.BaseFolderPath = configuration["OpenData:baseFolderPath"];
        }

        #endregion

    }

    public class FootballJSONConfig
    {

        #region Properties

        public string BaseDataFolderPath { get; set; }
        public string BaseFolderPath { get; set; }

        #endregion

        #region Constructors

        public FootballJSONConfig(IConfiguration configuration)
        {
            this.BaseDataFolderPath = configuration["FootballJSON:baseDataFolderPath"];
            this.BaseFolderPath = configuration["FootballJSON:baseFolderPath"];            
        }

        #endregion
    }

    public class FootballDataConfig
    {

        #region Properties

        public string BaseFolderPath { get; set; }
        public string Key { get; set; }

        public string MatchesUrl { get; set; }

        #endregion

        #region Properties

        public FootballDataConfig(IConfiguration configuration)
        {
            this.BaseFolderPath = configuration["FootballData:baseFolderPath"];
            this.Key = configuration["FootballData:key"];
            this.MatchesUrl = configuration["FootballData:matchesUrl"];
        }

        #endregion
    }

    public class TenserFlowConfig
    {

        #region Properties

        public string BaseTenserFlowPath { get; set; }
        public string ModelTenserFlowPath { get; set; }

        #endregion

        #region Constructors

        public TenserFlowConfig(IConfiguration configuration)
        {
            this.BaseTenserFlowPath = configuration["TenserFlow:baseTenserFlowPath"];
            this.ModelTenserFlowPath = configuration["TenserFlow:modelTenserFlowPath"];
        }

        #endregion
    }

    public class AppSettingsConfig
    {

        #region Properties

        public string BaseFolderPath { get; set; }        
        public string MatchFeaturesCSVFileName { get; set; }
        public string PredictionCSVFileName { get; set; }
        public bool TrainData { get; set; }
        public bool PredictData { get; set; }

        #endregion

        #region Constructors

        public AppSettingsConfig(IConfiguration configuration)
        {
            this.BaseFolderPath = configuration["AppSettings:baseFolderPath"];            
            this.MatchFeaturesCSVFileName = configuration["AppSettings:matchFeaturesCSVFileName"];           
            this.PredictionCSVFileName = configuration["AppSettings:predictionCSVFileName"];
            this.TrainData = Convert.ToBoolean(configuration["AppSettings:trainData"]);
            this.PredictData = Convert.ToBoolean(configuration["AppSettings:predictData"]);
        }

        #endregion

    }
}
