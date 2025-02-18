using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISoccerAPI.JSON.Merge.Data
{
    public class JSONMatch
    {
        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }
        public int HomeTeamGoals { get; set; }
        public int AwayTeamGoals { get; set; }
        public int MatchId { get; set; }
        public DateTime MatchPlayed { get; set; }
        public string Country { get; set; }
        public string Season { get; set; }
        public string Competition { get; set; }
    }
}
