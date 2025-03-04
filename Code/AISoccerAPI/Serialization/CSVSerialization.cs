using AISoccerAPI.API.SoccerAPI.SoccerLeagueDetail;
using AISoccerAPI.Calculation;
using AISoccerAPI.Data;
using AISoccerAPI.Train.TensorFlow.Callbacks;
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

        #region Load/Save Features

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

        #endregion

        #region Load/Save Predictions

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

        #endregion

        #region Load/Save TF Data

        public void SaveTFDataToCsv(List<TrainData> list, string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)))
            {
                csv.WriteRecords(list);
            }
        }

        public List<TrainData> LoadTrainDataFromCSV(string csvFilePath)
        {
            if (File.Exists(csvFilePath))
            {
                using (var reader = new StreamReader(csvFilePath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    return csv.GetRecords<TrainData>().ToList();
                }
            }
            else
                return new List<TrainData>();
        }

        #endregion
    }
}
