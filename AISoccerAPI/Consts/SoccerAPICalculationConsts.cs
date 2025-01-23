using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISoccerAPI.Consts
{
    internal class SoccerAPICalculationConsts
    {
        public const int Win = 3;
        public const int Draw = 1;
        public const int Lost = 0;
        public const double WeightFactor = 0.2;
        public const int FormMomentumMax = 5;
    }

    internal class MatchCategory
    {
        public const string Prediction = "Prediction";
        public const string Actual = "Actual";

    }
}
