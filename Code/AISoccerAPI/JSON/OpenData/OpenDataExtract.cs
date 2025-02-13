using AISoccerAPI.Consts;
using AISoccerAPI.Data;
using AISoccerAPI.JSON.OpenData.Data;
using Microsoft.ML.Trainers.FastTree;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISoccerAPI.JSON.OpenData
{
    public class OpenDataExtract
    {

        #region Constructors

        public OpenDataExtract()
        {

        }

        #endregion

        #region Public Methods

        public List<Competition> GetCompetitions(AppConfig appConfig)
        {
            List<Competition> toReturn = new List<Competition>();

            if (Directory.Exists(appConfig.OpenDataConfig.BaseFolderPath))
            {
                string filePath = appConfig.OpenDataConfig.BaseFolderPath + "competitions.json";

                if (File.Exists(filePath))
                {
                    // Read JSON content from file
                    string competitionJson = File.ReadAllText(filePath);

                    // Deserialize JSON into a list of Competition objects
                    List<Competition> competitions = JsonConvert.DeserializeObject<List<Competition>>(competitionJson);
                    toReturn.AddRange(competitions);
                }
            }

            return toReturn;
        }

        public Dictionary<string, string> GetAbbrevationForCountries()
        {
            Dictionary<string, string> toReturn = new Dictionary<string, string>();
            toReturn.Add("Germany", "de");
            toReturn.Add("Spain", "es");
            toReturn.Add("England", "en");
            toReturn.Add("Argentina", "ar");
            toReturn.Add("France", "fr");
            toReturn.Add("Italy", "it");
            return toReturn;
        }

        public Dictionary<string, List<(int competitionId, List<Match> matches)>> OpenDataExtractPrepareData(AppConfig appConfig)
        {
            Dictionary<string, List<(int competitionId, List<Match> matches)>> competitionMatchesByCountry =
                        new Dictionary<string, List<(int competitionId, List<Match> matches)>>();

            var competitions = GetCompetitions(appConfig);
            Dictionary<string, List<Competition>> competitionsByCountry = new Dictionary<string, List<Competition>>();
            var countries = competitions.Select(x => x.CountryName).Distinct().ToList();
            countries.ForEach(x =>
            {
                var countryCompetitions = competitions.FindAll(y => y.CountryName == x);
                competitionsByCountry.Add(x, countryCompetitions);
            });


            foreach (var keyValuePair in competitionsByCountry)
            {
                //use only competitions that are related to country not continent,
                //and avoid cup competitions and internatonal competitions
                //based on competition id and season id in data folder find json file
                //deserialize it, collect jsons, group them by country/league and generate MatchFeatures list 
                //based on all jsons for one league, league seasons must be under 2021. year
                if (!new Exclusions().GetContinents().Contains(keyValuePair.Key))
                {
                    Dictionary<int, List<string>> matchesJsonFilePathsByCompetition = new Dictionary<int, List<string>>();

                    foreach (var competition in keyValuePair.Value)
                    {
                        if (competition.CompetitionGender == "male" &&
                            !competition.CompetitionInternational &&
                            (competition.SeasonName.Contains("/") ?
                                Convert.ToInt32(competition.SeasonName.Split(new char[1] { '/' })[0]) < 2021 :
                                Convert.ToInt32(competition.SeasonName) < 2021) &&
                                !IsCupCompetition(competition.CompetitionName))
                        {
                            var matchesFolder = appConfig.OpenDataConfig.BaseFolderPath + "matches\\";
                            if (Directory.Exists(matchesFolder))
                            {
                                var jsonFilePath = matchesFolder + competition.CompetitionId + "\\" + competition.SeasonId + ".json";
                                if (File.Exists(jsonFilePath))
                                {
                                    if (matchesJsonFilePathsByCompetition.ContainsKey(competition.CompetitionId))
                                        matchesJsonFilePathsByCompetition[competition.CompetitionId].Add(jsonFilePath);
                                    else
                                        matchesJsonFilePathsByCompetition.Add(competition.CompetitionId, new List<string> { jsonFilePath });
                                }
                            }
                        }
                    }

                    foreach (var competitionPathKeyValuePair in matchesJsonFilePathsByCompetition)
                        foreach (var seasonPath in competitionPathKeyValuePair.Value)
                        {
                            string seasonJson = File.ReadAllText(seasonPath);
                            var seasonMatches = JsonConvert.DeserializeObject<List<Match>>(seasonJson);
                            var competition = competitions.FirstOrDefault(x => x.CompetitionId == competitionPathKeyValuePair.Key);
                            if (competition != null && competition.CountryName != "North and Central America")
                            {
                                if (competitionMatchesByCountry.ContainsKey(competition.CountryName))
                                {
                                    var matchesBySeason = competitionMatchesByCountry[competition.CountryName];
                                    if (matchesBySeason.FindAll(x => x.competitionId == competitionPathKeyValuePair.Key).Count == 0)
                                        matchesBySeason.Add((competitionPathKeyValuePair.Key, seasonMatches));
                                    else
                                    {
                                        var matchedSeasonMatches = matchesBySeason.Find(x => x.competitionId == competitionPathKeyValuePair.Key);
                                        matchedSeasonMatches.matches.AddRange(seasonMatches);
                                    }
                                }
                                else
                                {
                                    competitionMatchesByCountry.Add(competition.CountryName,
                                        new List<(int competitionId, List<Match> matches)> { (competitionPathKeyValuePair.Key, seasonMatches) });
                                }
                            }
                        }
                }
            }

            return competitionMatchesByCountry;
        }

        #endregion

        #region Private Methods

        private bool IsCupCompetition(string competitionName)
        {
            if (competitionName.Trim().ToLower().Contains("cup") ||
                competitionName.Trim().ToLower().Contains("copa"))
                return true;
            else
                return false;
        }

        #endregion

    }
}
