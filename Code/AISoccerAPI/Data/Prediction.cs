﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISoccerAPI.Data
{
    public class Prediction
    {
        public TeamPrediction Home { get; set; }
        public TeamPrediction Away { get; set; }
    }

    public class TeamPrediction
    {
        public string TeamName { get; set; }
        public float Goals { get; set; }
    }

    public class MatchPredictionResult
    {
        public string Category { get; set; }
        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }
        public float HomeTeamGoals { get; set; }
        public float AwayTeamGoals { get; set; }
        public float TotalGoals { get; set; }

        public string DatePlayed { get; set; }
    }
}