using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISoccerAPI.API.FootballData.Data
{
    // Define response models
    public class MatchesResponse
    {
        public Match[] Matches { get; set; }
    }

    public class Match
    {
        public string UtcDate { get; set; }
        public Team HomeTeam { get; set; }
        public Team AwayTeam { get; set; }
    }

    public class Team
    {
        public string Name { get; set; }
    }

}
