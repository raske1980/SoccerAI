using Newtonsoft.Json;
using System.Collections.Generic;

public class Parameters
{
    [JsonProperty("league")]
    public string League { get; set; }

    [JsonProperty("season")]
    public string Season { get; set; }
}

public class APIFixturePaging
{
    [JsonProperty("current")]
    public int Current { get; set; }

    [JsonProperty("total")]
    public int Total { get; set; }
}

public class Periods
{
    [JsonProperty("first")]
    public long First { get; set; }

    [JsonProperty("second")]
    public long Second { get; set; }
}

public class Venue
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("city")]
    public string City { get; set; }
}

public class Status
{
    [JsonProperty("long")]
    public string Long { get; set; }

    [JsonProperty("short")]
    public string Short { get; set; }

    [JsonProperty("elapsed")]
    public int Elapsed { get; set; }

    [JsonProperty("extra")]
    public object Extra { get; set; }
}

public class Fixture
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("referee")]
    public string Referee { get; set; }

    [JsonProperty("timezone")]
    public string Timezone { get; set; }

    [JsonProperty("date")]
    public string Date { get; set; }

    [JsonProperty("timestamp")]
    public long Timestamp { get; set; }

    [JsonProperty("periods")]
    public Periods Periods { get; set; }

    [JsonProperty("venue")]
    public Venue Venue { get; set; }

    [JsonProperty("status")]
    public Status Status { get; set; }
}

public class APIFixtureLeague
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("country")]
    public string Country { get; set; }

    [JsonProperty("logo")]
    public string Logo { get; set; }

    [JsonProperty("flag")]
    public string Flag { get; set; }

    [JsonProperty("season")]
    public int Season { get; set; }

    [JsonProperty("round")]
    public string Round { get; set; }

    [JsonProperty("standings")]
    public bool Standings { get; set; }
}

public class Home
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("logo")]
    public string Logo { get; set; }

    [JsonProperty("winner")]
    public bool? Winner { get; set; }
}

public class Away
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("logo")]
    public string Logo { get; set; }

    [JsonProperty("winner")]
    public bool? Winner { get; set; }
}

public class Teams
{
    [JsonProperty("home")]
    public Home Home { get; set; }

    [JsonProperty("away")]
    public Away Away { get; set; }
}

public class Goals
{
    [JsonProperty("home")]
    public int Home { get; set; }

    [JsonProperty("away")]
    public int Away { get; set; }
}

public class Halftime
{
    [JsonProperty("home")]
    public int Home { get; set; }

    [JsonProperty("away")]
    public int Away { get; set; }
}

public class Fulltime
{
    [JsonProperty("home")]
    public int Home { get; set; }

    [JsonProperty("away")]
    public int Away { get; set; }
}

public class Extratime
{
    [JsonProperty("home")]
    public object Home { get; set; }

    [JsonProperty("away")]
    public object Away { get; set; }
}

public class Penalty
{
    [JsonProperty("home")]
    public object Home { get; set; }

    [JsonProperty("away")]
    public object Away { get; set; }
}

public class Score
{
    [JsonProperty("halftime")]
    public Halftime Halftime { get; set; }

    [JsonProperty("fulltime")]
    public Fulltime Fulltime { get; set; }

    [JsonProperty("extratime")]
    public Extratime Extratime { get; set; }

    [JsonProperty("penalty")]
    public Penalty Penalty { get; set; }
}

public class APIFixtureResponse
{
    [JsonProperty("fixture")]
    public Fixture Fixture { get; set; }

    [JsonProperty("league")]
    public APIFixtureLeague League { get; set; }

    [JsonProperty("teams")]
    public Teams Teams { get; set; }

    [JsonProperty("goals")]
    public Goals Goals { get; set; }

    [JsonProperty("score")]
    public Score Score { get; set; }
}

public class FootbalAPIFixtureResponse
{
    [JsonProperty("get")]
    public string Get { get; set; }

    [JsonProperty("parameters")]
    public Parameters Parameters { get; set; }

    [JsonProperty("errors")]
    public List<object> Errors { get; set; }

    [JsonProperty("results")]
    public int Results { get; set; }

    [JsonProperty("paging")]
    public APIFixturePaging Paging { get; set; }

    [JsonProperty("response")]
    public List<APIFixtureResponse> Response { get; set; }
}