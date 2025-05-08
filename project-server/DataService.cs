using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using project_model;
using project_server.Dtos;
using project_server.Hubs;

namespace project_server;

public class DataService(IHubContext<DataHub> hubContext, ModelContext context, DataAnalysisService analysisService)
{
    private readonly IHubContext<DataHub> _hubContext = hubContext;
    private readonly ModelContext _context = context;

    public async Task UpdateData()
    {
        var entries = await _context.SubmittedValues.ToListAsync();
        var groupedData = entries
            .GroupBy(e => e.EntryOrigin)
            .Select(g => new EntryDTO
            {
                OriginID = g.Key,
                SubmittedValues = g.Select(v => new GroupedDataDTO
                {
                    Id = v.SubmissionID,
                    r_s = v.RateStars,
                    f_p = v.FrequencyPlanets,
                    n_e = v.NearEarth,
                    f_l = v.FractionLife,
                    f_i = v.FractionIntelligence,
                    f_c = v.FractionCommunication,
                    l = v.Length
                }).ToList()
            }).ToList();

        await _hubContext.Clients.All.SendAsync("updatedData", groupedData);
    }
}
