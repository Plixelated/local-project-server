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
        public async Task<ActionResult<Values>> PostSubmission(Values submission)
        {
            _context.SubmittedValues.Add(submission);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSubmission", new {id = submission.SubmissionID}, submission);

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
