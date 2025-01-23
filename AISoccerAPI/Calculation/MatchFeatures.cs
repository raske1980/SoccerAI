using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISoccerAPI.Calculation
{
    public class MatchFeatures
    {
        [LoadColumn(0)]
        public int MatchId { get; set; }
        [LoadColumn(1)]
        public string HomeTeam { get; set; }
        [LoadColumn(2)]
        public string AwayTeam { get; set; }
        [LoadColumn(3)]
        public string Date { get; set; }
        [LoadColumn(4)]
        public double GoalDifference { get; set; }
        [LoadColumn(5)]
        public double WinRateHome { get; set; }
        [LoadColumn(6)]
        public double WinRateAway { get; set; }
        [LoadColumn(7)]
        public double FormMomentumHome { get; set; }
        [LoadColumn(8)]
        public double FormMomentumAway { get; set; }
        [LoadColumn(9)]
        public double LeagueRankDifference { get; set; }
        [LoadColumn(10)]
        public float HomeGoals { get; set; }
        [LoadColumn(11)]
        public float AwayGoals { get; set; }
    }

    public class MatchPrediction
    {
        [ColumnName("Score")] public float PredictedGoals { get; set; }
    }
}


