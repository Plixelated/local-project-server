using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using project_model;
using project_server.Dtos;
using project_server.Hubs;
using System.Collections.Generic;
using System.Data;

namespace project_server.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Policy = "ViewUserData")]
    [ApiController]
    public class DataController(ModelContext context, DataAnalysisService analysisService) : ControllerBase
    {
        private readonly ModelContext _context = context;
        private readonly DataAnalysisService _analysisService = analysisService;

        [HttpGet("GetRawData")]
        public async Task<ActionResult<IEnumerable<EntryDTO>>> GetRawData()
        {

            var entries = await _context.SubmittedValues.ToListAsync();
            var groupedData = entries
                .GroupBy(e => e.EntryOrigin)
                .Select(g => new EntryDTO
                {
                    OriginID = g.Key,
                    SubmittedValues = g.Select(v => new GroupedDataDTO
                  {
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
                
            return groupedData;
        }


        [HttpGet("{origin}")]
        public async Task<ActionResult<Values>> GetUserSubmissions(string origin)
        {
            var submission = await _context.SubmittedValues.FindAsync(origin);

            if (submission == null)
                return NotFound();

            return submission;
        }

        [HttpPost("GetFlatDataset")] //use a Json with filters for this
        public async Task<ActionResult<Values>> GetFlatDataset(Dtos.DataFilterDTO filters)
        {
            List<Values> data = await _context.SubmittedValues.ToListAsync();
            List<FilteredData> dataset = _analysisService.FilterDataSet(data, filters.VariableFilter);
            FlatData results = _analysisService.FlattenData(dataset, filters.OperationFilter);

            return Ok(results);
        }

        [HttpPost("GetFilteredDataset")] //use a Json with filters for this
        public async Task<ActionResult<Values>> GetFilteredDataset(Dtos.DataFilterDTO filters)
        {

            List<Values> data = await _context.SubmittedValues.ToListAsync();
            List<FilteredData> dataset = _analysisService.FilterDataSet(data, filters.VariableFilter);
            List<AggregateData> results = _analysisService.PerformOperation(dataset, filters.OperationFilter);

            return Ok(results);
        }

        [HttpPost("GetAllData")] //use a Json with filters for this
        public async Task<ActionResult<Values>> GetAllData(Dtos.DataFilterDTO filters)
        {
            if (filters.VariableFilter.ToLower() == "all")
            {
                string[] variables = ["r_s", "f_p", "n_e", "f_l", "f_i", "f_c", "l"];

                List<System.Object> values = new List<System.Object>();

                List<Values> data = await _context.SubmittedValues.ToListAsync();

                foreach (var variable in variables)
                {
                    List<FilteredData> filteredData = _analysisService.FilterDataSet(data, variable);
                    List<AggregateData> aggData = _analysisService.PerformOperation(filteredData, filters.OperationFilter);
                    values.Add(aggData);
                }


                return Ok(values);
            }
            else{ return BadRequest("Invalid Options"); }
        }

        [HttpPost("GetUserSubmissions/{user}")] //use a Json with filters for this
        public async Task<ActionResult<Values>> GetAllData(Dtos.DataFilterDTO filters)
        {
            if (filters.VariableFilter.ToLower() == "all")
            {
                string[] variables = ["r_s", "f_p", "n_e", "f_l", "f_i", "f_c", "l"];

                List<System.Object> values = new List<System.Object>();

                List<Values> data = await _context.SubmittedValues
                    .Where(e => e.EntryOrigin == )
                    .ToListAsync();

                foreach (var variable in variables)
                {
                    List<FilteredData> filteredData = _analysisService.FilterDataSet(data, variable);
                    List<AggregateData> aggData = _analysisService.PerformOperation(filteredData, filters.OperationFilter);
                    values.Add(aggData);
                }


                return Ok(values);
            }
            else { return BadRequest("Invalid Options"); }
        }
    }
}
