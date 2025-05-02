using project_model;
using project_server.Dtos;

namespace project_server;

public class FilteredData
{
    public decimal Value { get; set;}
    public string OriginID { get; set;}
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
            }).ToList(),
            "f_p" => data.Select(v => new FilteredData
            {
                Value = v.FrequencyPlanets,
                OriginID = v.EntryOrigin,
            }).ToList(),
            "n_e" => data.Select(v => new FilteredData
            {
                Value = Convert.ToDecimal(v.NearEarth), 
                OriginID = v.EntryOrigin,
            }).ToList(),
            "f_l" => data.Select(v => new FilteredData
            {
                Value = v.FractionLife, 
                OriginID = v.EntryOrigin,
            }).ToList(),
            "f_i" => data.Select(v => new FilteredData
            {
                Value = v.FractionIntelligence, 
                OriginID = v.EntryOrigin,
            }).ToList(),
            "f_c" => data.Select( v=> new FilteredData
            {
                Value = v.FractionCommunication, 
                OriginID = v.EntryOrigin,
            }).ToList(),
            "l" => data.Select(v => new FilteredData
            {
                Value = Convert.ToDecimal(v.Length), 
                OriginID = v.EntryOrigin,
            }).ToList(),
            _ => throw new ArgumentException("Invalid Filter Option")
        };

    }

    public class AggregateData
    {
        public decimal Value { get; set; }
        public string OriginID { get; set; }
    }

    public List<AggregateData> PerformOperation(List<FilteredData> filteredData, string operation)
    {
        return operation switch
        {
            "min" => filteredData
            .GroupBy(f => f.OriginID)
            .Select(group => new AggregateData
            {
                OriginID = group.Key,
                Value = group.Min(f => f.Value)
            }).ToList(),

            "max" => filteredData
            .GroupBy(f => f.OriginID)
            .Select(group => new AggregateData
            {
                OriginID = group.Key,
                Value = group.Max(f => f.Value)
            }).ToList(),

            "avg" => filteredData
            .GroupBy(f => f.OriginID)
            .Select(group => new AggregateData
            {
                OriginID = group.Key,
                Value = group.Average(f => f.Value)
            }).ToList(),
            _ => throw new ArgumentException("Invalid Operation")
        };
    }

}

