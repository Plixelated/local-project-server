using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project_model;
using project_server.Dtos;

namespace project_server;

public class SubmissionSeedService(ModelContext context)
{

    private readonly ModelContext _context = context;

    public Entry CreateEntry(ValuesDTO valuesDto)
    {

        var newEntry = new Entry
        {
            Origin = valuesDto.EntryOrigin,
            SubmittedValues = new List<Values>
                {
                    new Values
                    {
                        RateStars = valuesDto.RateStars,
                        FrequencyPlanets = valuesDto.FrequencyPlanets,
                        NearEarth = valuesDto.NearEarth,
                        FractionLife = valuesDto.FractionLife,
                        FractionIntelligence = valuesDto.FractionIntelligence,
                        FractionCommunication = valuesDto.FractionCommunication,
                        Length = valuesDto.Length
                    }
                }
        };


        return newEntry;
    }


    public Values AddNewSubmission(ValuesDTO valuesDto)
    {
        var newSubmission = new Values
        {
            RateStars = valuesDto.RateStars,
            FrequencyPlanets = valuesDto.FrequencyPlanets,
            NearEarth = valuesDto.NearEarth,
            FractionLife = valuesDto.FractionLife,
            FractionIntelligence = valuesDto.FractionIntelligence,
            FractionCommunication = valuesDto.FractionCommunication,
            Length = valuesDto.Length,
            EntryOrigin = valuesDto.EntryOrigin
        };

        return newSubmission;
        
    }
}
