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
                if (filePath.EndsWith("Home.zip"))
                {
                    File.Delete(filePath);
                    break;
                }                 
            }
            foreach (var filePath in Directory.GetFiles(path))
            {
                if (filePath.Contains("Away.zip"))
                {
                    File.Delete(filePath);
                    break;
                }
            }

            //save new models
            mlContext.Model.Save(homeModel, trainingData.Schema, path + "\\" + "Home.zip");
            mlContext.Model.Save(awayModel, trainingData.Schema, path + "\\" + "Away.zip");
        }

        public (ITransformer loadedHomeModel, ITransformer loadedAwayModel) LoadModels(string path)
        {
            var mlContext = new MLContext();

            //load latest models (Home and Away)
            var allHomeModels = Directory.GetFiles(path).ToList().Where(x=> x.EndsWith("Home.zip")).Select(x => x).ToList();
            var allAwayModels = Directory.GetFiles(path).ToList().Where(x => x.Contains("Away.zip")).Select(x => x).ToList();

            string latestHomeModel = string.Empty;
            string latestAwayModel = string.Empty;
            
            foreach(var homeModelPath in allHomeModels)
            {
                var fileInfo = new FileInfo(homeModelPath);
                string fileName = fileInfo.Name;
                                                    
            }

            foreach (var awayModelPath in allAwayModels)
            {
                var fileInfo = new FileInfo(awayModelPath);
                string fileName = fileInfo.Name;
            }            

            //load models
            ITransformer loadedHomeModel = mlContext.Model.Load(path + "\\" + "Home.zip", out var homeModelInputSchema);
            ITransformer loadedAwayModel = mlContext.Model.Load(path + "\\" + "Away.zip", out var awayModelInputSchema);
            return (loadedHomeModel, loadedAwayModel);
        }
    }
}
