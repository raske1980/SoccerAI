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
            var footballAPIFeatures = new CSVSerialization().
                LoadFeaturesFromCSV(appConfig.FootballAPIConfig.BaseFolderPath + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName);

            DirectoryInfo di = new DirectoryInfo(appConfig.SoccerAPIConfig.BaseFolderPath);
            List<FileInfo> files = new List<FileInfo>();
            foreach (var file in di.GetFiles())
                if (file.Name.EndsWith("_MatchFeatures.csv")) files.Add(file);

            List<DateTime> dates = new List<DateTime>();
            foreach (var file in files)
            {
                var fileName = file.Name;
                var fileNameArr = fileName.Split(new char[1] { '_' });
                int year = Int32.Parse(fileNameArr[0].Substring(0,4));
                int month = Int32.Parse(fileNameArr[0].Substring(4, 2));
                int day = Int32.Parse(fileNameArr[0].Substring(6, 2));
                DateTime fileDateTime = new DateTime(year, month, day);
                dates.Add(fileDateTime);
            }

            dates.Sort();
            string soccerAPICSVFilePath = string.Empty;
            if (dates.Count > 0)
            {
                var file = files.Find(x => x.Name.StartsWith(dates[dates.Count - 1].ToString("yyyyMMdd")));
                if(file != null)
                    soccerAPICSVFilePath = file.FullName;
            }

            List<MatchFeatures> soccerAPIFeatures = new List<MatchFeatures>();
            if(!string.IsNullOrEmpty(soccerAPICSVFilePath))
                soccerAPIFeatures = new CSVSerialization().LoadFeaturesFromCSV(soccerAPICSVFilePath);

            footballAPIFeatures.AddRange(soccerAPIFeatures);
            new CSVSerialization().SaveFeaturesToCsv(footballAPIFeatures, appConfig.AppSettingsConfig.BaseFolderPath + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName);
        }
    }
}
