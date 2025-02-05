using AISoccerAPI.API.SoccerAPI.SoccerLeagueDetail;
using AISoccerAPI.Calculation;
using AISoccerAPI.Data;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISoccerAPI.Serialization
{
    public class CSVSerialization
    {
        public void SaveFeaturesToCsv(List<MatchFeatures> features,
            string csvFilePath)
        {            
            if (File.Exists(csvFilePath)) 
                File.Delete(csvFilePath);
            using (var writer = new StreamWriter(csvFilePath))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)))
            {
                csv.WriteRecords(features);
            }
        }

        public List<MatchFeatures> LoadFeaturesFromCSV(string csvFilePath)
        {
            if (File.Exists(csvFilePath))
            {
                using (var reader = new StreamReader(csvFilePath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    return csv.GetRecords<MatchFeatures>().ToList();                    
                }
            }
            else
                return new List<MatchFeatures>();
        }

        public void SaveMatchPredictionsToCsv(List<MatchPredictionResult> predictions, string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)))
            {
                csv.WriteRecords(predictions);
            }
        }
    }
}
