using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project_model;
using project_server.Dtos;

namespace project_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubmissionController(ModelContext context) : ControllerBase
    {
        //Values is located in context
        //submission is handling these values
        private readonly ModelContext _context = context;

        //https://go.microsoft.com/fwlink/?linkid=2123754

        [HttpPost]
        public async Task<ActionResult<Entry>> CreateEntry([FromBody] ValuesDTO valuesDto)
        {

            var origin = Guid.NewGuid().ToString();

            var newEntry = new Entry
            {
                Origin = origin,
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


            _context.Entries.Add(newEntry);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Submission saved.",
                origin = newEntry.Origin
            });
        }

        [HttpPut]
        public async Task<ActionResult<Values>> AddNewSubmission([FromBody] ValuesDTO valuesDto)
        {
            //Check if entry exists with current origin
            var existingEntry = await _context.Entries
                .Include(e => e.SubmittedValues)
                .FirstOrDefaultAsync(e => e.Origin == valuesDto.EntryOrigin);

            if (existingEntry == null)
            {
                return NotFound("No matching entry found");
            }

            var newSubmission = new Values
            {
                RateStars = valuesDto.RateStars,
                FrequencyPlanets = valuesDto.FrequencyPlanets,
                NearEarth = valuesDto.NearEarth,
                FractionLife = valuesDto.FractionLife,
                FractionIntelligence = valuesDto.FractionIntelligence,
                FractionCommunication = valuesDto.FractionCommunication,
                Length = valuesDto.Length
            };

            existingEntry?.SubmittedValues.Add(newSubmission);
            await _context.SaveChangesAsync();
            return Ok("Sumbission Added Succesfully");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Values>>> GetSubmissions()
        {
            return await _context.SubmittedValues.Take(100).ToListAsync();
        }


        // GET: api/Cities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Values>> GetSubmission(int id)
        {
            var submission = await _context.SubmittedValues.FindAsync(id);

            if (submission == null)
            {
                return NotFound();
            }

            return submission;
        }
    }
}
