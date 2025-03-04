using AISoccerAPI.Data;
using Tensorflow;
using Tensorflow.Keras.Engine;

namespace AISoccerAPI.Train.TensorFlow
{
    public class SaveLoadTFModel
    {
        // ✅ Save Model
        public void SaveModel(IModel model, AppConfig appConfig)
        {
            model.save(appConfig.TenserFlowConfig.ModelTenserFlowPath);            
        }

        // ✅ Load Model
        public IModel LoadModel(AppConfig appConfig)
        {
            var model = KerasApi.keras.models.load_model(appConfig.TenserFlowConfig.ModelTenserFlowPath); // ✅ Correct way to load            
            return model;
        }
    }
}
