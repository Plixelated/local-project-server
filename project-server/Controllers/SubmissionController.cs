using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using project_model;
using project_server.Dtos;
using project_server.Hubs;

namespace project_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubmissionController(
        ModelContext context, 
        DataService dataService, 
        SubmissionSeedService submissionSeedService
        ) : ControllerBase
    {
        //Values is located in context
        //submission is handling these values
        private readonly ModelContext _context = context;
        private readonly DataService _dataService = dataService;
        private readonly SubmissionSeedService _submissionSeedService = submissionSeedService;

        [HttpPost("CreateEntry")]
        public async Task<ActionResult<Entry>> CreateEntry([FromBody] ValuesDTO valuesDto)
        {

            var newEntry = _submissionSeedService.CreateEntry(valuesDto);

            _context.Entries.Add(newEntry);
            await _context.SaveChangesAsync();
            //Send SignalR Update
            await _dataService.UpdateData();

            return Ok(new
            {
                message = "Submission saved.",
                origin = newEntry.Origin
            });
        }

        [HttpPost("CreateSubmission")]
        public async Task<ActionResult<Values>> AddNewSubmission([FromBody] ValuesDTO valuesDto)
        {
            //Check if entry exists with current origin
            var existingEntry = await _context.Entries
                .Include(e => e.SubmittedValues)
                .FirstOrDefaultAsync(e => e.Origin == valuesDto.EntryOrigin);

            if (existingEntry == null)
            {
                await CreateEntry(valuesDto);
            }

            var newSubmission = _submissionSeedService.AddNewSubmission(valuesDto);

            existingEntry?.SubmittedValues.Add(newSubmission);
            await _context.SaveChangesAsync();
            //Send SignalR Update
            await _dataService.UpdateData();

            return Ok("Sumbission Added Succesfully");
        }


    }
}
