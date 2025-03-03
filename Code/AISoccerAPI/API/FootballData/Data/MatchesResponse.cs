using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISoccerAPI.API.FootballData.Data
{
    public class MatchesResponseRes
    {
        public MatchRes[] Matches { get; set; }
    }

    public class MatchRes
    {
        public string UtcDate { get; set; }
        public TeamRes HomeTeam { get; set; }
        public TeamRes AwayTeam { get; set; }
        public Score Score { get; set; }
    }

    public class TeamRes
    {
        public string Name { get; set; }
    }

    public class Score
    {
        public FullTime FullTime { get; set; }
    }

    public class FullTime
    {
        public int? Home { get; set; }
        public int? Away { get; set; }
    }
}
