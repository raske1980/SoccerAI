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

        #region Constructors

        public MergeMultipleSources()
        {

        }

        #endregion


        #region Methods

        public void MergeFeatures(AppConfig appConfig)
        {
            MergeAPIFeatures(appConfig);

            MergeJSONFeatures(appConfig);

            MergeAllFeatures(appConfig);
        }

        #region Private Methods

        private void MergeAllFeatures(AppConfig appConfig)
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
            FixValues(apiFeatures);
            new CSVSerialization().SaveFeaturesToCsv(apiFeatures, appConfig.AppSettingsConfig.BaseFolderPath + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName);
        }
        
        private void MergeAPIFeatures(AppConfig appConfig)
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

        private void MergeJSONFeatures(AppConfig appConfig)
        {
            //load json match features
            var parentFolder = new DirectoryInfo(appConfig.OpenDataConfig.BaseFolderPath).Parent.FullName;
            var jsonFeatures = new CSVSerialization().
                    LoadFeaturesFromCSV(parentFolder + "\\" + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName);
            var apiFeatures = new CSVSerialization().
                    LoadFeaturesFromCSV(appConfig.AppSettingsConfig.BaseFolderPath + "API\\" + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName);
            HashSet<MatchFeatures> apiHashSet = new HashSet<MatchFeatures>(apiFeatures);            

            //removing duplicates
            List<MatchFeatures> toRemove = new List<MatchFeatures>();
            jsonFeatures.RemoveAll(x => apiHashSet.Contains(x));                        

            new CSVSerialization().SaveFeaturesToCsv(jsonFeatures,
                appConfig.AppSettingsConfig.BaseFolderPath + "JSON\\" + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName);            
        }

        private void FixValues(List<MatchFeatures> allFeatures)
        {
            foreach (var feature in allFeatures)
            {
                if (Double.IsNaN(feature.GoalDifference) || Double.IsInfinity(feature.GoalDifference))
                    feature.GoalDifference = 0d;

                if (Double.IsNaN(feature.FormMomentumHome) || Double.IsInfinity(feature.FormMomentumHome))
                    feature.FormMomentumHome = 0d;

                if (Double.IsNaN(feature.LeagueRankDifference) || Double.IsInfinity(feature.LeagueRankDifference))
                    feature.LeagueRankDifference = 0d;

                if (Single.IsNaN(feature.AwayGoals) || Single.IsInfinity(feature.AwayGoals))
                    feature.AwayGoals = 0f;

                if (Double.IsNaN(feature.FormMomentumAway) || Double.IsInfinity(feature.FormMomentumAway))
                    feature.FormMomentumAway = 0d;

                if (Single.IsNaN(feature.HomeGoals) || Single.IsInfinity(feature.HomeGoals))
                    feature.HomeGoals = 0f;

                if (Double.IsNaN(feature.GoalDifference) || Double.IsInfinity(feature.GoalDifference))
                    feature.GoalDifference = 0d;

                if (Double.IsNaN(feature.WinRateAway) || Double.IsInfinity(feature.WinRateAway))
                    feature.WinRateAway = 0d;

                if (Double.IsNaN(feature.WinRateHome) || Double.IsInfinity(feature.WinRateHome))
                    feature.WinRateHome = 0d;
            }
        }

        #endregion

        #endregion
    }
}
