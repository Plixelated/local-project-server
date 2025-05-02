using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project_model;
using project_server.Dtos;

namespace project_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController(ModelContext context) : ControllerBase
    {
        private readonly ModelContext _context = context;

        [Authorize(Roles = "Admin")]
        [HttpGet("GetAllValues")]
        public async Task<ActionResult<IEnumerable<Values>>> GetSubmissions()
        {
            //return await _context.SubmittedValues.Take(100).ToListAsync();
            return await _context.SubmittedValues.ToListAsync();
        }


        [Authorize(Roles = "Admin,User")]
        [HttpGet("{origin}")]
        public async Task<ActionResult<Values>> GetSubmission(string origin)
        {
            var submission = await _context.SubmittedValues.FindAsync(origin);

            if (submission == null)
                return NotFound();

            return submission;
        }
    }
}
