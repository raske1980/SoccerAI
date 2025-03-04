using AISoccerAPI.Data;
using AISoccerAPI.Train.ML;
using AISoccerAPI.Train.TensorFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISoccerAPI.Train
{
    public class Train
    {
        #region Properties

        #endregion

        #region Constructors

        public Train()
        {

        }

        #endregion

        #region Methods

        public void TrainModels(AppConfig appConfig)
        {
            //train data based on data from all sources
            new TrainMLModel().StartTrainModel(appConfig);

            new TrainTFModel().TrainModel(appConfig);
        }

        #endregion
    }
}
