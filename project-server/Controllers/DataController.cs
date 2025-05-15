using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using project_model;
using project_server.Dtos;
using project_server.Hubs;
using System.Collections.Generic;
using System.Data;

//This Controller handles are requests for Data from the
//database. Specifically handles retrieval and performing
//caluclations and data operations to be used in the frontend.

namespace project_server.Controllers
{
    //All API's are restricted to only those who have a User Account
    [Route("api/[controller]")]
    [Authorize(Policy = "UserOnlyAccess")]
    [ApiController]
    public class DataController(
        ModelContext context, DataAnalysisService analysisService, UserManager<ProjectUser> userManager) : ControllerBase
    {
        private readonly ModelContext _context = context;
        private readonly DataAnalysisService _analysisService = analysisService;

        //Additionally Restriced to Users who can ViewUserData
        [Authorize(Policy = "ViewUserData")] //<- rename ViewUserSubmissions or ViewAllData?
        [HttpGet("GetRawData")]
        public async Task<ActionResult<IEnumerable<EntryDTO>>> GetRawData()
        {
            //Get All Values
            var entries = await _context.SubmittedValues.ToListAsync();

            //Create a List of Lists that help to organize data.
            //This ensures that the retrieved data is organized
            //by originID
            var groupedData = entries
                .GroupBy(e => e.EntryOrigin) //Group by OriginID
                .Select(g => new EntryDTO
                {
                    //Create a new list of EntryDTO that contains
                    //all the submitted values correspnding to the originID
                    OriginID = g.Key,
                    SubmittedValues = g.Select(v => new GroupedDataDTO
                  {
                      //Create a new list of GroupedDataDTO to handle
                      //all Submitted Values by that originID
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

        //Returns submissions by OriginID
        [HttpGet("{origin}")]
        public async Task<ActionResult<Values>> GetUserSubmissions(string origin)
        {
            var submission = await _context.SubmittedValues.FindAsync(origin);

            if (submission == null)
                return NotFound();

            return submission;
        }

        //Performs operations to flatten the dataset across all values
        //This is the total calculation is displayed in the user Dashboard
        [HttpPost("GetFlatDataset")]
        public async Task<ActionResult<Values>> GetFlatDataset(Dtos.DataFilterDTO filters)
        {
            //Retrieve all the values from the database
            List<Values> data = await _context.SubmittedValues.ToListAsync();
            //Filter that data according to what variable (for flattened data its "all")
            List<FilteredData> dataset = _analysisService.FilterDataSet(data, filters.VariableFilter);
            //Flatten the data according to the specified operation: Min, Max, Avg
            FlatData results = _analysisService.FlattenData(dataset, filters.OperationFilter);

            return Ok(results);
        }

        //Allows user to filter the dataset by specific variable
        //Additionally Restriced to Users who can ViewUserData
        [Authorize(Policy = "ViewUserData")]
        [HttpPost("GetFilteredDataset")]
        public async Task<ActionResult<Values>> GetFilteredDataset(Dtos.DataFilterDTO filters)
        {
            //Retrieve all the values from the database
            List<Values> data = await _context.SubmittedValues.ToListAsync();
            //Filter that data according to what variable( R_s, F_p..etc)
            List<FilteredData> dataset = _analysisService.FilterDataSet(data, filters.VariableFilter);
            //Aggregate the data according to the specified operation: Min, Max, Avg
            List<AggregateData> results = _analysisService.PerformOperation(dataset, filters.OperationFilter);

            return Ok(results);
        }

        //Retreives All of the Data
        //Additionally Restriced to Users who can ViewUserData
        [Authorize(Policy = "ViewUserData")]
        [HttpPost("GetAllData")]
        public async Task<ActionResult<Values>> GetAllData(Dtos.DataFilterDTO filters)
        {
            if (filters.VariableFilter.ToLower() == "all")
            {
                //Create List of Variables
                string[] variables = ["r_s", "f_p", "n_e", "f_l", "f_i", "f_c", "l"];
                //Create List of generic object
                List<System.Object> values = new List<System.Object>();
                //Retrieve all the values from the database
                List<Values> data = await _context.SubmittedValues.ToListAsync();

                //Loop through filter the data
                foreach (var variable in variables)
                {
                    //Although no filter is provided since it is "all" it converts each entry
                    //To the FilteredData Object so it can be processed into aggregate data
                    List<FilteredData> filteredData = _analysisService.FilterDataSet(data, variable);
                    //Aggregate the data according to the specified operation: Min, Max, Avg
                    List<AggregateData> aggData = _analysisService.PerformOperation(filteredData, filters.OperationFilter);
                    //Add it to the generic List
                    values.Add(aggData);
                }

                return Ok(values);
            }
            else{ return BadRequest("Invalid Options"); }
        }

        //Retrieves Specific Users Submission
        [HttpGet("GetUserSubmissions")]
        public async Task<ActionResult<IEnumerable<EntryDTO>>> GetUserData()
        {
            //Get requesting User
            var userName = User?.Identity?.Name;
            //Find User Info
            var userInfo = await userManager.FindByNameAsync(userName);
            //Extract User ID
            var userID = userInfo!.Id;
            //Return error if no user was found
            if (userInfo == null)
                return BadRequest("User Not Found");
            //Find User Origin Link
            var link = await _context.UserOrigin.FirstOrDefaultAsync(e => e.UserId == userID);
            //Retreive User's OriginID
            var originID = link?.EntryOrigin;
            //If not found, return error
            if (originID == null)
                return BadRequest("OriginID Not Found");
            //Find all values related to that user's originID
            var entry = await _context.SubmittedValues.Where(s => s.EntryOrigin == originID).ToListAsync();
            //Group the Data by thier OriginID
            var groupedData = entry
                .GroupBy(e => e.EntryOrigin)
                .Select(g => new EntryDTO
                {
                    //Create a list of GroupedDataDTO to hand all submitted values
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
    }
}
