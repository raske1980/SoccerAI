using AISoccerAPI.Calculation;
using AISoccerAPI.Data;
using AISoccerAPI.Serialization;
using AISoccerAPI.Train.TensorFlow.Callbacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Tensorflow;
using Tensorflow.Keras;
using Tensorflow.Keras.ArgsDefinition;
using Tensorflow.Keras.Callbacks;
using Tensorflow.Keras.Engine;
using Tensorflow.Keras.Layers;
using Tensorflow.Keras.Losses;
using Tensorflow.Keras.Models;
using Tensorflow.Keras.Optimizers;
using Tensorflow.NumPy;
using static Tensorflow.Binding;
using static Tensorflow.KerasApi;

namespace AISoccerAPI.Train.TensorFlow
{
    public class TrainTFModel
    {
        #region Properties

        #endregion

        #region Constructors

        #endregion

        #region Methods

        public void TrainModel(AppConfig appConfig)
        {
            var allMatchFeatures = new CSVSerialization().
                    LoadFeaturesFromCSV(new DirectoryInfo(appConfig.AppSettingsConfig.BaseFolderPath) + appConfig.AppSettingsConfig.MatchFeaturesCSVFileName);

            if (allMatchFeatures == null || allMatchFeatures.Count == 0)            
                throw new Exception("No match data available for training.");

            int epochCount = appConfig.TenserFlowConfig.EpochCount;
            int batchSize = appConfig.TenserFlowConfig.BatchSize;
            float lr = appConfig.TenserFlowConfig.LearningRate;

            DateTime parsedDate = DateTime.MinValue;
            allMatchFeatures.OrderByDescending(x => { DateTime.TryParse(x.Date, out parsedDate); return parsedDate; });

            (NDArray trainX, NDArray trainY, NDArray testX, NDArray testY) = PrepareData(allMatchFeatures);
            var model = BuildModel();
            model.compile(optimizer: new Adam(learning_rate: lr),
              loss: new MeanSquaredError(),
              metrics: new[] { "mae" });
            
            var callbackParams = new CallbackParams
            {
                Model = model,
                Epochs = epochCount,                
            };
            var logCallBack = new LogCallback(callbackParams, appConfig);

            model.fit(trainX, trainY, batch_size: batchSize, epochs: epochCount, validation_data: (testX, testY), callbacks: new List<ICallback> { logCallBack });
            new SaveLoadTFModel().SaveModel(model, appConfig);
        }

        #region Private Methods

        private IModel BuildModel()
        {
            var inputs = keras.layers.Input(shape: new Shape(6)); // Input layer with 6 features

            // First dense layer with 64 neurons and ReLU activation
            var x = new Dense(new DenseArgs { Units = 64, Activation = keras.activations.Relu }).Apply(inputs);

            // Second dense layer with 32 neurons and ReLU activation
            x = new Dense(new DenseArgs { Units = 32, Activation = keras.activations.Relu }).Apply(x);

            // Output layer with 2 neurons (HomeGoals, AwayGoals) and linear activation
            var outputs = new Dense(new DenseArgs { Units = 2, Activation = keras.activations.Linear }).Apply(x);

            // Create and return the model
            IModel model = keras.Model(inputs, outputs, name: "match_prediction_model");
            model.summary();

            return model;
        }

        private (NDArray, NDArray, NDArray, NDArray) PrepareData(List<MatchFeatures> matches)
        {
            if (matches == null || matches.Count == 0)
            {
                throw new Exception("Error: No match data available for training.");
            }

            // Convert input features to a 2D float array
            var inputData = matches.Select(m => new float[]
            {
                (float)m.GoalDifference,
                (float)m.WinRateHome,
                (float)m.WinRateAway,
                (float)m.FormMomentumHome,
                (float)m.FormMomentumAway,
                (float)m.LeagueRankDifference
            }).ToArray();

            // Convert output labels to a 2D float array
            var outputData = matches.Select(m => new float[]
            {
                m.HomeGoals,
                m.AwayGoals
            }).ToArray();

            Console.WriteLine($"Total samples: {inputData.Length}");
            Console.WriteLine($"Each sample should have 6 features.");

            foreach (var row in inputData)
            {
                if (row.Length != 6)
                {
                    throw new Exception($"Error: Found a row with {row.Length} features instead of 6.");
                }
            }

            int splitIndex = (int)(matches.Count * 0.8); // 80% train, 20% test

            // Convert inputData (float[][]) to a proper 2D array (float[,])
            int rows = inputData.Length;
            int cols = inputData[0].Length;
            float[,] reshapedInputData = new float[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    reshapedInputData[i, j] = inputData[i][j];
                }
            }

            // Convert outputData (float[][]) to a proper 2D array (float[,])
            int outputRows = outputData.Length;
            int outputCols = outputData[0].Length;
            float[,] reshapedOutputData = new float[outputRows, outputCols];

            for (int i = 0; i < outputRows; i++)
            {
                for (int j = 0; j < outputCols; j++)
                {
                    reshapedOutputData[i, j] = outputData[i][j];
                }
            }

            // Slice input and output arrays manually
            float[,] trainX = new float[splitIndex, cols];
            float[,] testX = new float[rows - splitIndex, cols];
            float[,] trainY = new float[splitIndex, outputCols];
            float[,] testY = new float[outputRows - splitIndex, outputCols];

            for (int i = 0; i < splitIndex; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    trainX[i, j] = reshapedInputData[i, j];
                }
                for (int j = 0; j < outputCols; j++)
                {
                    trainY[i, j] = reshapedOutputData[i, j];
                }
            }

            for (int i = splitIndex; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    testX[i - splitIndex, j] = reshapedInputData[i, j];
                }
                for (int j = 0; j < outputCols; j++)
                {
                    testY[i - splitIndex, j] = reshapedOutputData[i, j];
                }
            }

            // Convert to NDArray for TensorFlow
            var trainXND = np.array(trainX, dtype: np.float32);
            var trainYND = np.array(trainY, dtype: np.float32);
            var testXND = np.array(testX, dtype: np.float32);
            var testYND = np.array(testY, dtype: np.float32);

            return (trainXND, trainYND, testXND, testYND);
        }
        
        #endregion

        #endregion
    }
}
