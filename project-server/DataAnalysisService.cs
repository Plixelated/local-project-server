using project_model;
using project_server.Dtos;

namespace project_server;

public class FilteredData
{
    public decimal Value { get; set;}
    public string OriginID { get; set;}
    public string Field { get; set;}
}

public class DataAnalysisService
{
    public List<FilteredData> FilterDataSet(List<Values> data,string filter)
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
                Value = Convert.ToDecimal(v.NearEarth), 
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
            "f_c" => data.Select( v=> new FilteredData
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
                new FilteredData { Value = v.RateStars, OriginID = v.EntryOrigin, Field = "r_s"},
                new FilteredData { Value = v.FrequencyPlanets, OriginID = v.EntryOrigin, Field = "f_P" },
                new FilteredData { Value = Convert.ToDecimal(v.NearEarth), OriginID = v.EntryOrigin, Field = "n_e" },
                new FilteredData { Value = v.FractionLife, OriginID = v.EntryOrigin, Field = "f_l" },
                new FilteredData { Value = v.FractionIntelligence, OriginID = v.EntryOrigin, Field = "f_i" },
                new FilteredData { Value = v.FractionCommunication, OriginID = v.EntryOrigin, Field = "f_c"},
                new FilteredData { Value = Convert.ToDecimal(v.Length), OriginID = v.EntryOrigin, Field = "l" }
            }).ToList(),
            _ => throw new ArgumentException("Invalid Filter Option")
        };

    }

    public class AggregateData
    {
        public decimal Value { get; set; }
        public string OriginID { get; set; }
        public string Field { get; set; }
    }

    public List<AggregateData> PerformOperation(List<FilteredData> filteredData, string operation)
    {
        return operation switch
        {
            "min" => filteredData
            .GroupBy(f => new {f.OriginID, f.Field})
            .Select(group => new AggregateData
            {
                OriginID = group.Key.OriginID,
                Value = group.Min(f => f.Value),
                Field = group.Key.Field
            }).ToList(),

            "max" => filteredData
            .GroupBy(f => new { f.OriginID, f.Field })
            .Select(group => new AggregateData
            {
                OriginID = group.Key.OriginID,
                Value = group.Max(f => f.Value),
                Field = group.Key.Field
            }).ToList(),

            "avg" => filteredData
            .GroupBy(f => new { f.OriginID, f.Field })
            .Select(group => new AggregateData
            {
                OriginID = group.Key.OriginID,
                Value = group.Average(f => f.Value),
                Field = group.Key.Field
            }).ToList(),
            _ => throw new ArgumentException("Invalid Operation")
        };
    }

}

