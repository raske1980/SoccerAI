using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISoccerAPI.ML
{
    public class SaveLoadModel
    {
        public void SaveModel(string path,
            ITransformer homeModel, 
            ITransformer awayModel,
            IDataView trainingData)
        {
            var mlContext = new MLContext();

            //delete model files if they exist
            foreach (var filePath in Directory.GetFiles(path)) {
                if (filePath.Contains(DateTime.Now.ToString("yyyyMMdd") + "_" + "Home"))
                {
                    File.Delete(filePath);
                    break;
                }                 
            }
            foreach (var filePath in Directory.GetFiles(path))
            {
                if (filePath.Contains(DateTime.Now.ToString("yyyyMMdd") + "_" + "Away"))
                {
                    File.Delete(filePath);
                    break;
                }
            }

            //save new models
            mlContext.Model.Save(homeModel, trainingData.Schema, path + DateTime.Now.ToString("yyyyMMdd") + "_" + "Home.zip");
            mlContext.Model.Save(awayModel, trainingData.Schema, path + DateTime.Now.ToString("yyyyMMdd") + "_" + "Away.zip");
        }

        public (ITransformer loadedHomeModel, ITransformer loadedAwayModel) LoadModels(string path)
        {
            var mlContext = new MLContext();

            //load latest models (Home and Away)
            var allHomeModels = Directory.GetFiles(path).ToList().Where(x=> x.Contains("Home")).Select(x => x).ToList();
            var allAwayModels = Directory.GetFiles(path).ToList().Where(x => x.Contains("Away")).Select(x => x).ToList();

            string latestHomeModel = string.Empty;
            string latestAwayModel = string.Empty;

            List<DateTime> homeTimeList = new List<DateTime>();
            foreach(var homeModelPath in allHomeModels)
            {
                var fileInfo = new FileInfo(homeModelPath);
                string fileName = fileInfo.Name;
                var fileNameArray = fileName.Split(new char[1] { '_' });
                if(fileNameArray.Length > 1)
                {
                    int year = Int32.Parse(fileNameArray[1].Substring(0, 4));
                    int month = Int32.Parse(fileNameArray[1].Substring(4, 2));
                    int day = Int32.Parse(fileNameArray[1].Substring(6, 2));
                    DateTime homeDateTime = new DateTime(year, month, day); 
                    homeTimeList.Add(homeDateTime);
                }                                    
            }

            List<DateTime> awayTimeList = new List<DateTime>();
            foreach (var awayModelPath in allAwayModels)
            {
                var fileInfo = new FileInfo(awayModelPath);
                string fileName = fileInfo.Name;
                var fileNameArray = fileName.Split(new char[1] { '_' });
                if (fileNameArray.Length > 1)
                {
                    int year = Int32.Parse(fileNameArray[1].Substring(0, 4));
                    int month = Int32.Parse(fileNameArray[1].Substring(4, 2));
                    int day = Int32.Parse(fileNameArray[1].Substring(6, 2));
                    DateTime homeDateTime = new DateTime(year, month, day);
                    awayTimeList.Add(homeDateTime);
                }
            }
            //sort models to take latest
            homeTimeList.Sort();
            awayTimeList.Sort();

            //load models
            ITransformer loadedHomeModel = mlContext.Model.Load(path + homeTimeList[homeTimeList.Count - 1].ToString("yyyyMMdd") + "_" + "Home.zip", out var homeModelInputSchema);
            ITransformer loadedAwayModel = mlContext.Model.Load(path + awayTimeList[awayTimeList.Count - 1].ToString("yyyyMMdd") + "_" + "Away.zip", out var awayModelInputSchema);
            return (loadedHomeModel, loadedAwayModel);
        }
    }
}
