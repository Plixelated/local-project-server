using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using project_model;
using project_server.Dtos;
using project_server.Hubs;

namespace project_server;

//Separate Data Service for SignalR Updates

public class DataService(IHubContext<DataHub> hubContext, ModelContext context, DataAnalysisService analysisService)
{
    private readonly IHubContext<DataHub> _hubContext = hubContext;
    private readonly ModelContext _context = context;

    //Create UpdateData Task for SignalR
    public async Task UpdateData()
    {
        //Retrieve All Entries
        var entries = await _context.SubmittedValues.ToListAsync();

        //Create a List of Lists that help to organize data.
        //This ensures that the retrieved data is organized
        //by originID
        var groupedData = entries
            .GroupBy(e => e.EntryOrigin) //Group by OriginID
            .Select(g => new EntryDTO
            {
                //Create a new list of EntryDTO that contains
                //all the submitted values correspnding to the originID
                OriginID = g.Key,
                SubmittedValues = g.Select(v => new GroupedDataDTO
                {
                    //Create a new list of GroupedDataDTO to handle
                    //all Submitted Values by that originID
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

        //Sends the most up to data data from the database via the SignalR Hub
        //this allows the frontend to have a live reference of the most up to date
        //data in the database by connecting to the updatedData connection
        await _hubContext.Clients.All.SendAsync("updatedData", groupedData);
    }
}
