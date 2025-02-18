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
