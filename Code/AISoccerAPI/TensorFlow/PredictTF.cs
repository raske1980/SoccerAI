using AISoccerAPI.Calculation;
using Tensorflow; // ✅ Required for tf.constant
using Tensorflow.Keras.Engine;
using static Tensorflow.Binding; // ✅ Required for using 'tf'

namespace AISoccerAPI.TensorFlow
{
    public class PredictTF
    {
        public void Predict(MatchFeatures matchFeature, IModel model)
        {
            var newMatch = new float[,] { {
                (float)matchFeature.GoalDifference,
                (float)matchFeature.WinRateHome,
                (float)matchFeature.WinRateAway,
                (float)matchFeature.FormMomentumHome,
                (float)matchFeature.FormMomentumAway,
                (float)matchFeature.LeagueRankDifference
            }};

            var inputTensor = tf.constant(newMatch); // ✅ Convert to tensor
            var prediction = model.predict(inputTensor); // ✅ Predict

            // Convert prediction tensor to NumSharp array
            var predictionArray = prediction.numpy(); // ✅ Fix indexing issue

            // Access prediction values correctly
            Console.WriteLine($"Predicted Tenser Flow Home Goals For {matchFeature.HomeTeam}: {predictionArray[0][0]}");
            Console.WriteLine($"Predicted Tenser Flow Away Goals For {matchFeature.AwayTeam}: {predictionArray[0][1]}");
        }
    }
}
