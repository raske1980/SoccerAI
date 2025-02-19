using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISoccerAPI.JSON.FootballJSON.Data
{
    public class Match
    {
        public string round { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public string team1 { get; set; }
        public string team2 { get; set; }
        public Score score { get; set; }

        public Match(Match match)
        {
            this.round = match.round;
            this.date = match.date;
            this.time = match.time;
            this.team1 = match.team1;
            this.team2 = match.team2;
            this.score = match.score;
        }

        public Match(Match2 match)
        {
            this.date = match.date;
            this.round = string.Empty;
            this.score = match.score;
            this.team1 = match.team1;
            this.team2 = match.team2;
            this.time = string.Empty;
        }
    }

    public class MatchExt : Match
    {
        public string season { get; set; }
        public MatchExt(string season, Match match) 
            : base(match)
        {
            this.season = season;
        }
    }

    public class RootFootballJSON
    {
        public string name { get; set; }
        public List<Match> matches { get; set; }
    }

    public class Score
    {
        public List<int> ht { get; set; }
        public List<int> ft { get; set; }
    }

    #region Json2

    public class Match2
    {
        public string date { get; set; }
        public string team1 { get; set; }
        public string team2 { get; set; }
        public Score score { get; set; }
    }

    public class RootFootballJSON2
    {
        public string name { get; set; }
        public List<Round2> rounds { get; set; }
    }

    public class Round2
    {
        public string name { get; set; }
        public List<Match2> matches { get; set; }
    }

    public class Score2
    {
        public List<int> ft { get; set; }
    }


    #endregion
}
