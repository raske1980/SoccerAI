using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AISoccerAPI.Calculation
{
    public class MatchFeatures
    {
        #region Properties

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

        #endregion

        #region Constructors

        public MatchFeatures()
        {

        }

        public MatchFeatures(MatchFeatures matchFeatures)
        {
            this.MatchId = matchFeatures.MatchId;
            this.Date = matchFeatures.Date;            
            this.HomeTeam = matchFeatures.HomeTeam;
            this.AwayTeam = matchFeatures.AwayTeam;
            this.AwayGoals = matchFeatures.AwayGoals;
            this.HomeGoals = matchFeatures.HomeGoals;
            this.FormMomentumAway = matchFeatures.FormMomentumAway;
            this.FormMomentumHome = matchFeatures.FormMomentumHome;
            this.GoalDifference = matchFeatures.GoalDifference;
            this.LeagueRankDifference = matchFeatures.LeagueRankDifference;
            this.WinRateAway = matchFeatures.WinRateAway;
            this.WinRateHome = matchFeatures.WinRateHome;
        }

        #endregion

        #region Methods

        #region Overrides

        public override bool Equals(object? obj)
        {
            if (obj != null)
            {
                var matchFeature = obj as MatchFeatures;
                if (matchFeature != null)                
                    return this.HomeTeam == matchFeature.HomeTeam &&
                            this.AwayTeam == matchFeature.AwayTeam &&
                            this.Date == matchFeature.Date;                
                else
                    return false;
            }
            else
                return false;
        }

        public override int GetHashCode()
        {
            return this.Date.GetHashCode() + this.HomeTeam.GetHashCode() + this.AwayTeam.GetHashCode();
        }

        #endregion

        #endregion
    }

    public class MatchFeatureExt : MatchFeatures
    {
        public DateTime ParsedDateTime { get; set; }
        public MatchFeatureExt(MatchFeatures matchFeature) : base(matchFeature)
        {
            DateTime parsedDate = DateTime.MinValue;            
            string[] dateArr = matchFeature.Date.Split(new char[1] { '/' });            
            if (dateArr.Length > 2)
                parsedDate = new DateTime(Int32.Parse(dateArr[2]), Int32.Parse(dateArr[1]), Int32.Parse(dateArr[0]));

            this.ParsedDateTime = parsedDate;
        }
    }

    public class MatchPrediction
    {
        [ColumnName("Score")] public float PredictedGoals { get; set; }
    }
}


