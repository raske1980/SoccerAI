using AISoccerAPI.Calculation;
using AISoccerAPI.Data;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISoccerAPI.Serialization
{
    public class CSVSerialization
    {
        public void SaveFeaturesToCsv(List<MatchFeatures> features, string filePath)
        {
            if(File.Exists(filePath)) 
                File.Delete(filePath);
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)))
            {
                csv.WriteRecords(features);
            }
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
