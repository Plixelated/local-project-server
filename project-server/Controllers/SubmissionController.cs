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

        [Authorize(Policy="ManageData")]
        [HttpDelete("Delete/{submissionID}")]
        public async Task<IActionResult> DeleteSubmission(int submissionID)
        {
            var entry = await _context.SubmittedValues.FindAsync(submissionID);
            if (entry == null)
            {
                return NotFound();
            }

            _context.SubmittedValues.Remove(entry);
            await _context.SaveChangesAsync();

            //Send SignalR Update
            await _dataService.UpdateData();

            return NoContent();
        }

        [Authorize(Policy="ManageData")]
        [HttpPut("Edit")]
        public async Task<IActionResult> EditSubmission(EditDataDTO editData)
        {

            //WHERE ID = editData.id;
            var entity = _context.SubmittedValues.FirstOrDefault(e => e.SubmissionID == editData.Id);

            if(entity == null)
                return NotFound();

            //SET entity.value = editData.value
            entity.RateStars = editData.r_s;
            entity.FrequencyPlanets = editData.f_p;
            entity.NearEarth = (short)editData.n_e;
            entity.FractionLife = editData.f_l;
            entity.FractionIntelligence = editData.f_i;
            entity.FractionCommunication = editData.f_c;
            entity.Length = (long)editData.l;

            _context.Entry(entity).Property(e => e.RateStars).IsModified = true;
            _context.Entry(entity).Property(e => e.FrequencyPlanets).IsModified = true;
            _context.Entry(entity).Property(e => e.NearEarth).IsModified = true;
            _context.Entry(entity).Property(e => e.FractionLife).IsModified = true;
            _context.Entry(entity).Property(e => e.FractionIntelligence).IsModified = true;
            _context.Entry(entity).Property(e => e.FractionCommunication).IsModified = true;
            _context.Entry(entity).Property(e => e.Length).IsModified = true;

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

        private bool SubmissionExists(int id)
        {
            return _context.SubmittedValues.Any(e => e.SubmissionID == id);
        }


    }

}
