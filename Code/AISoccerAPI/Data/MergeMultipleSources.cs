using AISoccerAPI.Calculation;
using AISoccerAPI.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AISoccerAPI.Data
{
    public class MergeMultipleSources
    {
        public void MergeFeatures(AppConfig appConfig)
        {
            MergeAPIFeatures(appConfig);

            MergeJSONFeatures(appConfig);

            MergeAllFeatures(appConfig);
        }

        public void MergeAllFeatures(AppConfig appConfig)
        {
            //load api fetures
            var apiFeatures = new List<MatchFeatures>();
            if (File.Exists(new DirectoryInfo(appConfig.FootballAPIConfig.BaseFolderPath).Parent.FullName + "\\" + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName))
                apiFeatures = new CSVSerialization().
                    LoadFeaturesFromCSV(new DirectoryInfo(appConfig.FootballAPIConfig.BaseFolderPath).Parent.FullName + "\\" + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName);

            //load json features
            var jsonFeatures = new List<MatchFeatures>();
            if (File.Exists(new DirectoryInfo(appConfig.FootballAPIConfig.BaseFolderPath).Parent.Parent.FullName + "\\JSON\\" + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName))
                jsonFeatures = new CSVSerialization().
                    LoadFeaturesFromCSV(new DirectoryInfo(appConfig.FootballAPIConfig.BaseFolderPath).Parent.Parent.FullName + "\\JSON\\" + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName);

            //union of all features and write them down to main folder
            apiFeatures.AddRange(jsonFeatures);
            new CSVSerialization().SaveFeaturesToCsv(apiFeatures, appConfig.AppSettingsConfig.BaseFolderPath + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName);
        }

        public void MergeAPIFeatures(AppConfig appConfig)
        {
            //load first football api features
            var footballAPIFeatures = new List<MatchFeatures>();
            if (File.Exists(appConfig.FootballAPIConfig.BaseFolderPath + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName))
                footballAPIFeatures = new CSVSerialization().
                    LoadFeaturesFromCSV(appConfig.FootballAPIConfig.BaseFolderPath + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName);

            //load second soccer api features
            var soccerAPIFeatures = new List<MatchFeatures>();
            if (File.Exists(appConfig.SoccerAPIConfig.BaseFolderPath + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName))
                soccerAPIFeatures = new CSVSerialization().
                    LoadFeaturesFromCSV(appConfig.SoccerAPIConfig.BaseFolderPath + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName);

            //make union and write them to the API folder
            footballAPIFeatures.AddRange(soccerAPIFeatures);
            if (File.Exists(appConfig.AppSettingsConfig.BaseFolderPath + "API\\" + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName))
                File.Delete(appConfig.AppSettingsConfig.BaseFolderPath + "API\\" + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName);

            new CSVSerialization().SaveFeaturesToCsv(footballAPIFeatures, appConfig.AppSettingsConfig.BaseFolderPath + "API\\" + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName);
        }

        public void MergeJSONFeatures(AppConfig appConfig)
        {
            //load first open data
            var openDataFeatures = new List<MatchFeatures>();
            if(File.Exists(appConfig.OpenDataConfig.BaseFolderPath + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName))
                openDataFeatures = new CSVSerialization().
                    LoadFeaturesFromCSV(appConfig.FootballAPIConfig.BaseFolderPath + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName);

            //load second footbal json
            var footbalJSONFeatures = new List<MatchFeatures>();
            if(File.Exists(appConfig.FootballJSONConfig + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName))            
                footbalJSONFeatures = new CSVSerialization().
                    LoadFeaturesFromCSV(appConfig.SoccerAPIConfig.BaseFolderPath + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName);

            //make union and write them to the JSON folder
            openDataFeatures.AddRange(footbalJSONFeatures);
            if (File.Exists(appConfig.AppSettingsConfig.BaseFolderPath + "JSON\\" + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName))
                File.Delete(appConfig.AppSettingsConfig.BaseFolderPath + "JSON\\" + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName);

            new CSVSerialization().SaveFeaturesToCsv(openDataFeatures, 
                appConfig.AppSettingsConfig.BaseFolderPath + "JSON\\" + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName);
        }
    }
}
