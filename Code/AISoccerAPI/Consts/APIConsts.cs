using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AISoccerAPI.Consts
{
    public class APIConsts
    {
        public const int Win = 3;
        public const int Draw = 1;
        public const int Lost = 0;
        public const double WeightFactor = 0.2;
        public const int FormMomentumMax = 5;
        public const int MaxFootballAPIRequests = 30;        
    }

    public class MatchCategory
    {
        public const string Prediction = "Prediction";
        public const string Actual = "Actual";
    }

    public class Exclusions
    {
        public List<(string league, string country, string footballAPIMappingName)> GetSoccerApiLeaaguesByCountry()
        {
            return new List<(string league, string country, string footballAPIMappingName)> { 
                ("Tipico Bundesliga","Austria", "Bundesliga"),
                ("Jupiler League","Belgium","Jupiler Pro League"),
                ("1. Liga","Czech-Republic", "Czech Liga"),
                ("Superliga","Denmark", "Superliga"),
                ("Premier League","England", "Premier League"),
                ("Championship","England", "Championship"),
                ("League One","England", "League One"),
                ("League Two","England", "League Two"),
                ("Ligue 1","France", "Ligue 1"),
                ("Ligue 2","France", "Ligue 2"),
                ("Bundesliga","Germany", "Bundesliga"),
                ("2. Bundesliga","Germany", "2. Bundesliga"),
                ("Super League","Greece", "Super League 1"),
                ("Serie A","Italy", "Serie A"),
                ("Serie B","Italy", "Serie B"),
                ("Eredivisie","Netherlands", "Eredivisie"),
                ("Ekstraklasa","Poland", "Ekstraklasa"),
                ("Primeira Liga","Portugal", "Primeira Liga"),
                ("LaLiga","Spain", "La Liga"),
                ("LaLiga2","Spain", "Segunda División"),                
            };
        }
    }
}
