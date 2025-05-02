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
    public class DataController(ModelContext context, DataAnalysisService analysisService) : ControllerBase
    {
        private readonly ModelContext _context = context;
        private readonly DataAnalysisService _analysisService = analysisService;

        //[Authorize(Roles = "Admin")]
        [HttpGet("GetRawData")]
        public async Task<ActionResult<IEnumerable<Values>>> GetRawData()
        {

            var retrievedEntries = await _context.SubmittedValues.ToListAsync();
            return retrievedEntries;
        }


        [Authorize(Roles = "Admin,User")]
        [HttpGet("{origin}")]
        public async Task<ActionResult<Values>> GetUserSubmissions(string origin)
        {
            var submission = await _context.SubmittedValues.FindAsync(origin);

            if (submission == null)
                return NotFound();

            return submission;
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost("GetDataset")] //use a Json with filters for this
        public async Task<ActionResult<Values>> GetDataset(Dtos.DataFilterDTO filters)
        {
            //Handle request for all data with a seprate API call
            var data = await _context.SubmittedValues.ToListAsync();
            var dataset = _analysisService.FilterDataSet(data, filters.VariableFilter);
            var results = _analysisService.PerformOperation(dataset, filters.OperationFilter);

            return Ok(results);
        }
    }
}
