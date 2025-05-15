using project_model;
using project_server.Dtos;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace project_server;

//This Service handles performing data operations
//per DataController requests

//Internal Classes used to format data
public class FilteredData
{
    public decimal Value { get; set;}
    public string OriginID { get; set;}
    public string Field { get; set;}
}

public class AggregateData: FilteredData { }
public class FlatData
{
    public decimal r_s { get; set; }
    public decimal f_p { get; set; }
    public decimal n_e { get; set; }
    public decimal f_l { get; set; }
    public decimal f_i { get; set; }
    public decimal f_c { get; set; }
    public decimal l { get; set; }
}

public class DataAnalysisService
{
    //Takes in a list of values and uses switch expression to
    //filter data according to the requested variable
    //It includes the value, originID, and field (variable)
    public List<FilteredData> FilterDataSet(List<Values> data, string filter)
    {
        return filter switch
        {
            "r_s" => data.Select(v => new FilteredData
            {
                Value = v.RateStars,
                OriginID = v.EntryOrigin,
                Field = filter
            }).ToList(),
            "f_p" => data.Select(v => new FilteredData
            {
                Value = v.FrequencyPlanets,
                OriginID = v.EntryOrigin,
                Field = filter
            }).ToList(),
            "n_e" => data.Select(v => new FilteredData
            {
                Value = v.NearEarth,
                OriginID = v.EntryOrigin,
                Field = filter
            }).ToList(),
            "f_l" => data.Select(v => new FilteredData
            {
                Value = v.FractionLife,
                OriginID = v.EntryOrigin,
                Field = filter
            }).ToList(),
            "f_i" => data.Select(v => new FilteredData
            {
                Value = v.FractionIntelligence,
                OriginID = v.EntryOrigin,
                Field = filter
            }).ToList(),
            "f_c" => data.Select(v => new FilteredData
            {
                Value = v.FractionCommunication,
                OriginID = v.EntryOrigin,
                Field = filter
            }).ToList(),
            "l" => data.Select(v => new FilteredData
            {
                Value = Convert.ToDecimal(v.Length),
                OriginID = v.EntryOrigin,
                Field = filter
            }).ToList(),
            "all" => data.SelectMany(v => new List<FilteredData>
            {
                //If "All" is requested, it creates a list of Filtered Data for each Variable
                new FilteredData { Value = v.RateStars, OriginID = v.EntryOrigin, Field = "r_s"},
                new FilteredData { Value = v.FrequencyPlanets, OriginID = v.EntryOrigin, Field = "f_p" },
                new FilteredData { Value = v.NearEarth, OriginID = v.EntryOrigin, Field = "n_e" },
                new FilteredData { Value = v.FractionLife, OriginID = v.EntryOrigin, Field = "f_l" },
                new FilteredData { Value = v.FractionIntelligence, OriginID = v.EntryOrigin, Field = "f_i" },
                new FilteredData { Value = v.FractionCommunication, OriginID = v.EntryOrigin, Field = "f_c"},
                new FilteredData { Value = Convert.ToDecimal(v.Length), OriginID = v.EntryOrigin, Field = "l" }
            }).ToList(),
            _ => throw new ArgumentException("Invalid Filter Option")
        };

    }

    //Takes in a list of filtered data and uses
    //switch expressions and grouping to perform
    //aggregatin operations like Min, Max, Avg
    public List<AggregateData> PerformOperation(List<FilteredData> filteredData, string operation)
    {
        return operation switch
        {
            "min" => filteredData
            .GroupBy(f => new { f.OriginID, f.Field }) //Organize by originID and Field (variable)
            .Select(group => new AggregateData
            {
                OriginID = group.Key.OriginID, //Sets Origin ID
                Value = group.Min(f => f.Value), //Returns Min value
                Field = group.Key.Field //Sets Field (variable)
            }).ToList(),

            "max" => filteredData
            .GroupBy(f => new { f.OriginID, f.Field }) //Organize by originID and Field (variable)
            .Select(group => new AggregateData
            {
                OriginID = group.Key.OriginID,  //Sets Origin ID
                Value = group.Max(f => f.Value),  //Returns Max value
                Field = group.Key.Field //Sets Field (variable)
            }).ToList(),

            "avg" => filteredData
            .GroupBy(f => new { f.OriginID, f.Field }) //Organize by originID and Field (variable)
            .Select(group => new AggregateData
            {
                OriginID = group.Key.OriginID, //Sets Origin ID
                Value = group.Average(f => f.Value), //Returns Average value
                Field = group.Key.Field //Sets Field (variable)
            }).ToList(),
            _ => throw new ArgumentException("Invalid Operation")
        };
    }

    //Takes in a list of filtered data
    //And uses a combiantion of grouping
    //and a switch statement to reduce the dataset
    //to only a single value per variable
    public FlatData FlattenData(List<FilteredData> filteredData, string operation)
    {
        var fieldGroups = filteredData.GroupBy(f => f.Field); //Group By Variable
        var results = new FlatData();

        //Loop through each variable type
        foreach(var group in fieldGroups)
        {
            //Creates a switch operation to flatten the data
            var aggregate = operation switch
            {
                "min" => group.Min(var => var.Value), //Flattens according to Minimum values
                "max" => group.Max(var => var.Value), //Flattens according to Maximum values
                "avg" => group.Average(var => var.Value), //Flattens according to Average values
                _ => throw new ArgumentException("Invalid Operation")
            };

            //Recompile into the FlatData object
            switch (group.Key)
            {
                case "r_s": results.r_s = aggregate; break;
                case "f_p": results.f_p = aggregate; break;
                case "n_e": results.n_e = aggregate; break;
                case "f_l": results.f_l = aggregate; break;
                case "f_i": results.f_i = aggregate; break;
                case "f_c": results.f_c = aggregate; break;
                case "l": results.l = Convert.ToDecimal(aggregate); break;
            }
        }

        return results;

    }

}

