using AISoccerAPI.Data;
using AISoccerAPI.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tensorflow;
using Tensorflow.Keras.Callbacks;
using Tensorflow.Keras.Engine;

namespace AISoccerAPI.Train.TensorFlow.Callbacks
{
    public class LogCallback : ICallback
    {
        public Dictionary<string, List<float>> history { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public CallbackParams CallBackParams { get; set; }
        public AppConfig AppConfig;

        public LogCallback(CallbackParams callBackParams, AppConfig appConfig)
        {
            this.CallBackParams = callBackParams;
            this.AppConfig = appConfig;
        }

        public void on_epoch_begin(int epoch)
        {
            
        }

        public void on_epoch_end(int epoch, Dictionary<string, float> epoch_logs)
        {
            //val_loss: 1.383675 - val_mean_absolute_error: 0.930576   
            if(epoch_logs.ContainsKey("val_loss") && 
                epoch_logs.ContainsKey("mean_absolute_error") &&
                epoch_logs.ContainsKey("loss") &&
                epoch_logs.ContainsKey("val_mean_absolute_error") &&
                epoch == this.CallBackParams.Epochs - 1)
            {
                //last epoch, log loss and mae
                TrainData trainData = new TrainData
                {
                    Ticks = DateTime.Now.Ticks,
                    ValLoss = epoch_logs["val_loss"],
                    ValMAE = epoch_logs["val_mean_absolute_error"],
                    Loss = epoch_logs["loss"],
                    MAE= epoch_logs["mean_absolute_error"]
                };

                var logPath = this.AppConfig.TenserFlowConfig.BaseTenserFlowPath + this.AppConfig.TenserFlowConfig.LogTrainingValues;
                var trainingData = new CSVSerialization().
                                       LoadTrainDataFromCSV(logPath);
                trainingData.Add(trainData);
                new CSVSerialization().SaveTFDataToCsv(trainingData, logPath);
            }
        }

        public void on_predict_batch_begin(long step)
        {
            
        }

        public void on_predict_batch_end(long end_step, Dictionary<string, Tensors> logs)
        {
            
        }

        public void on_predict_begin()
        {
            
        }

        public void on_predict_end()
        {
            
        }

        public void on_test_batch_begin(long step)
        {
            
        }

        public void on_test_batch_end(long end_step, Dictionary<string, float> logs)
        {
            
        }

        public void on_test_begin()
        {
            
        }

        public void on_test_end(Dictionary<string, float> logs)
        {
            
        }

        public void on_train_batch_begin(long step)
        {
            
        }

        public void on_train_batch_end(long end_step, Dictionary<string, float> logs)
        {
            
        }

        public void on_train_begin()
        {
            
        }

        public void on_train_end()
        {
            
        }
    }

    public class TrainData
    {
        public float ValLoss { get; set; }
        public float Loss { get; set; }
        public float ValMAE { get; set; }
        public float MAE { get; set; }
        public long Ticks { get; set; }
    }
}
