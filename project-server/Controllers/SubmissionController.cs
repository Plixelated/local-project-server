using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using project_model;
using project_server.Dtos;
using project_server.Hubs;

//This controller handles Submissions to the database

namespace project_server.Controllers
{
    //Restricted to those with a user account
    [Authorize(Policy = "UserOnlyAccess")]
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

        //Allows anyone to submit
        [AllowAnonymous]
        [HttpPost("CreateEntry")]
        public async Task<ActionResult<Entry>> CreateEntry([FromBody] ValuesDTO valuesDto)
        {
            //Create new entry
            var newEntry = _submissionSeedService.CreateEntry(valuesDto);
            //Add entry to db
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

        //Allows anyone to submit
        [AllowAnonymous]
        [HttpPost("CreateSubmission")]
        public async Task<ActionResult<Values>> AddNewSubmission([FromBody] ValuesDTO valuesDto)
        {
            //Check if entry exists with current origin
            var existingEntry = await _context.Entries
                .Include(e => e.SubmittedValues)
                .FirstOrDefaultAsync(e => e.Origin == valuesDto.EntryOrigin);
            //If it doesn't
            if (existingEntry == null)
            {
                //create new entry instead
                await CreateEntry(valuesDto);
            }
            //If it does
            var newSubmission = _submissionSeedService.AddNewSubmission(valuesDto);
            //Add values to entry
            existingEntry?.SubmittedValues.Add(newSubmission);
            await _context.SaveChangesAsync();
            //Send SignalR Update
            await _dataService.UpdateData();

            return Ok("Sumbission Added Succesfully");
        }

        //Allow users to delete their sumbissions
        [HttpDelete("Delete/{submissionID}")]
        public async Task<IActionResult> DeleteSubmission(int submissionID)
        {
            //Find submission
            var entry = await _context.SubmittedValues.FindAsync(submissionID);
            //If it doesn't exist send an error message
            if (entry == null)
                return NotFound();
            //Submission from database
            _context.SubmittedValues.Remove(entry);
            await _context.SaveChangesAsync();

            //Send SignalR Update
            await _dataService.UpdateData();

            return NoContent();
        }

        //Allow users to edit their sumbissions
        [HttpPut("Edit")]
        public async Task<IActionResult> EditSubmission(EditDataDTO editData)
        {
            //Get Submission
            //WHERE ID = editData.id;
            var entity = _context.SubmittedValues.FirstOrDefault(e => e.SubmissionID == editData.Id);
            //If not found, return error
            if(entity == null)
                return NotFound();

            //SET entity.value = editData.value
            entity.RateStars = editData.r_s;
            entity.FrequencyPlanets = editData.f_p;
            entity.NearEarth = editData.n_e;
            entity.FractionLife = editData.f_l;
            entity.FractionIntelligence = editData.f_i;
            entity.FractionCommunication = editData.f_c;
            entity.Length = (long)editData.l;

            //Manually tell the database that these values have been modified
            _context.Entry(entity).Property(e => e.RateStars).IsModified = true;
            _context.Entry(entity).Property(e => e.FrequencyPlanets).IsModified = true;
            _context.Entry(entity).Property(e => e.NearEarth).IsModified = true;
            _context.Entry(entity).Property(e => e.FractionLife).IsModified = true;
            _context.Entry(entity).Property(e => e.FractionIntelligence).IsModified = true;
            _context.Entry(entity).Property(e => e.FractionCommunication).IsModified = true;
            _context.Entry(entity).Property(e => e.Length).IsModified = true;

            //Save changes, if any issues return a database concurrency error
            try
            {
                await _context.SaveChangesAsync();
                //Send SignalR Update
                await _dataService.UpdateData();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SubmissionExists(editData.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        //Check if the submission exists in the database
        private bool SubmissionExists(int id)
        {
            return _context.SubmittedValues.Any(e => e.SubmissionID == id);
        }


    }

}
