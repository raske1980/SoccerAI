using AISoccerAPI.Data;
using Tensorflow;
using Tensorflow.Keras.Engine;

namespace AISoccerAPI.TensorFlow
{
    public class SaveLoadTFModel
    {
        // ✅ Save Model
        public void SaveModel(IModel model, AppConfig appConfig)
        {
            model.save(appConfig.AppSettingsConfig.BaseFolderPath);            
        }

        // ✅ Load Model
        public IModel LoadModel(AppConfig appConfig)
        {
            var model = KerasApi.keras.models.load_model(appConfig.AppSettingsConfig.BaseFolderPath); // ✅ Correct way to load            
            return model;
        }
    }
}
