using System;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace AISoccerAPI.JSON.OpenData.Data
{
    public class AwayTeam
    {
        public int away_team_id { get; set; }
        public string away_team_name { get; set; }
        public string away_team_gender { get; set; }
        public object away_team_group { get; set; }
        public Country country { get; set; }
        public List<Manager> managers { get; set; }
    }

    public class MatchCompetition
    {
        public int competition_id { get; set; }
        public string country_name { get; set; }
        public string competition_name { get; set; }
    }

    public class CompetitionStage
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Country
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class HomeTeam
    {
        public int home_team_id { get; set; }
        public string home_team_name { get; set; }
        public string home_team_gender { get; set; }
        public object home_team_group { get; set; }
        public Country country { get; set; }
        public List<Manager> managers { get; set; }
    }

    public class Manager
    {
        public int id { get; set; }
        public string name { get; set; }
        public string nickname { get; set; }
        public string dob { get; set; }
        public Country country { get; set; }
    }

    public class Metadata
    {
        public string data_version { get; set; }
        public string shot_fidelity_version { get; set; }
        public string xy_fidelity_version { get; set; }
    }

    public class Referee
    {
        public int id { get; set; }
        public string name { get; set; }
        public Country country { get; set; }
    }

    public class Match
    {
        public int match_id { get; set; }
        public string match_date { get; set; }
        public string kick_off { get; set; }
        public MatchCompetition competition { get; set; }
        public Season season { get; set; }
        public HomeTeam home_team { get; set; }
        public AwayTeam away_team { get; set; }
        public int home_score { get; set; }
        public int away_score { get; set; }
        public string match_status { get; set; }
        public string match_status_360 { get; set; }
        public DateTime last_updated { get; set; }
        public DateTime? last_updated_360 { get; set; }
        public Metadata metadata { get; set; }
        public int match_week { get; set; }
        public CompetitionStage competition_stage { get; set; }
        public Stadium stadium { get; set; }
        public Referee referee { get; set; }
    }

    public class Season
    {
        public int season_id { get; set; }
        public string season_name { get; set; }
    }

    public class Stadium
    {
        public int id { get; set; }
        public string name { get; set; }
        public Country country { get; set; }
    }
}

