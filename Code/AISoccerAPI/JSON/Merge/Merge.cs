using AISoccerAPI.Calculation;
using AISoccerAPI.JSON.Merge.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AISoccerAPI.JSON.Merge
{
    public class Merge
    {
        #region Constructors

        public Merge()
        {

        }

        #endregion

        #region Public Methods

        public List<MatchFeatures> MergeAll(Dictionary<string, List<(int competitionId, List<OpenData.Data.Match> matches)>> openDataJSON,
            Dictionary<string, List<(string competition, List<AISoccerAPI.JSON.FootballJSON.Data.Match> matches)>> footballJSON)
        {
            var jsonMatches = MergeJSONData(openDataJSON, footballJSON);
            return GetMatchFeatures(jsonMatches);
        }

        public List<JSONMatch> MergeJSONData(Dictionary<string, List<(int competitionId, List<OpenData.Data.Match> matches)>> openDataJSON,
            Dictionary<string, List<(string competition, List<AISoccerAPI.JSON.FootballJSON.Data.Match> matches)>> footballJSON)
        {
            List<JSONMatch> toReturn = new List<JSONMatch>();
            //this method needs to normalize data from all sources, remove duplicates
            return toReturn;
        }

        public List<MatchFeatures> GetMatchFeatures(List<JSONMatch> matches)
        {
            List<MatchFeatures> matchFeatures = new List<MatchFeatures>();
            //this method needs to use normalized data and to create
            //match features that will also later when it is merged with
            //API features be filtered from duplicates
            return matchFeatures;
        }

        #endregion

        #region Private Methods

        #endregion

    }
}
