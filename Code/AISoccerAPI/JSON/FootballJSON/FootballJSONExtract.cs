using AISoccerAPI.Data;
using AISoccerAPI.JSON.FootballJSON.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISoccerAPI.JSON.FootballJSON
{
    public class FootballJSONExtract
    {
        public Dictionary<string, List<(string competition, List<AISoccerAPI.JSON.FootballJSON.Data.MatchExt> matches)>> PrepareData(AppConfig appConfig)
        {
            Dictionary<string, List<(string competition, List<AISoccerAPI.JSON.FootballJSON.Data.MatchExt> matches)>> toReturn =
                new Dictionary<string, List<(string competition, List<Data.MatchExt> matches)>>();

            var dataFolderPath = appConfig.FootballJSONConfig.BaseDataFolderPath;
            if (Directory.Exists(dataFolderPath))
            {
                var countryByAbbr = GetCountryByAbbr();
                var subDirectories = Directory.GetDirectories(dataFolderPath);
                foreach(var subDirectory in subDirectories)
                {
                    if (subDirectory.EndsWith(".git")) continue;
                    DirectoryInfo di = new DirectoryInfo(subDirectory);
                    var files = Directory.GetFiles(subDirectory);
                    foreach(var file in files)
                    {
                        FileInfo fi = new FileInfo(file);
                        if (fi.Name.StartsWith("cl") ||
                            fi.Name.StartsWith("mx") ||
                            fi.Name.Contains("cup") ||
                            fi.Name.Contains("quali") ||
                            fi.Name.Contains("clubs") ||
                            fi.Name.Contains("uefa"))
                            continue;

                        var json = File.ReadAllText(file);

                        //first serialization
                        RootFootballJSON league = JsonConvert.DeserializeObject<RootFootballJSON>(json);
                        RootFootballJSON2 league2 = null;
                        //if json isnt what we expected we try with another deserialization
                        if (league.matches == null)
                            league2 = JsonConvert.DeserializeObject<RootFootballJSON2>(json);
                        var country = countryByAbbr[fi.Name.Split(new char[1] { '.' })[0]];
                        var season = di.Name;
                        var matches = league.matches;
                        List<Match2> tempList = new List<Match2>();
                        if (matches == null && league2 != null)
                        {
                            matches = new List<Match>();
                            foreach (var item in league2.rounds)
                                tempList.AddRange(item.matches);
                            foreach (var item in tempList)
                                matches.Add(new Match(item));
                        }

                        List<AISoccerAPI.JSON.FootballJSON.Data.MatchExt> matchList = new List<MatchExt>();
                        foreach (var match in matches)
                            matchList.Add(new MatchExt(league.name, match));
                                                    
                        if(toReturn.ContainsKey(country))
                        {
                            if (toReturn[country].FindAll(x => x.competition == fi.Name.Split(new char[1] { '.' })[1]).Count == 0)
                                toReturn[country].Add((fi.Name.Split(new char[1] { '.' })[1], matchList));
                            else
                            {
                                var seasonData = toReturn[country].FirstOrDefault(x => x.competition == fi.Name.Split(new char[1] { '.' })[1]);
                                seasonData.matches.AddRange(matchList);
                            }
                        }
                        else                        
                            toReturn.Add(country, new List<(string competition, List<Data.MatchExt> matches)> { (fi.Name.Split(new char[1] { '.' })[1], matchList) });                        
                    }
                }
            }

            return toReturn;
        }

        public Dictionary<string, string> GetCountryByAbbr()
        {
            var toReturn = new Dictionary<string, string>();

            toReturn.Add("at", "Austria");
            toReturn.Add("au", "Australia");
            toReturn.Add("de", "Germany");
            toReturn.Add("en", "England");
            toReturn.Add("it", "Italy");
            toReturn.Add("fr", "France");
            toReturn.Add("es", "Spain");
            toReturn.Add("ar", "Argentina");
            toReturn.Add("ch", "Switzerland");
            toReturn.Add("cz", "Czech Republic");
            toReturn.Add("gr", "Greece");
            toReturn.Add("hu", "Hungary");
            toReturn.Add("nl", "Nederland");
            toReturn.Add("pt", "Portugal");
            toReturn.Add("ru", "Russia");
            toReturn.Add("sco", "Scotland");
            toReturn.Add("tr", "Turkey");
            toReturn.Add("br", "Brasil");
            toReturn.Add("cn", "China");
            toReturn.Add("jp", "Japan");
            toReturn.Add("be", "Belgium");

            return toReturn;
        }        
    }
}
