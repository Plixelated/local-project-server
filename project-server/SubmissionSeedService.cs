using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project_model;
using project_server.Dtos;

namespace project_server;

//This Service handles the creation of Entry and Value DTO objects
//By abstracting it into this service, the operations can be shared
//by the Submission Controller and Seed Contoller.


public class SubmissionSeedService()
{
    public Entry CreateEntry(ValuesDTO valuesDto)
    {
        //Creates and populates a new Entry
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
        //Creates and populates a new submission
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
