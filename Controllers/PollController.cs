using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PollMonitor.Repository;
using PollMonitor.Models;
using Newtonsoft;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace PollMonitor.Controllers
{
    [Route("api/[controller]/")]
    [ApiController]
    public class PollController : ControllerBase
    {
        private readonly ApplicationDbContext _database;
        public PollController(ApplicationDbContext database)
        {
            this._database = database;
        }

        [HttpGet("[id]")]
        public IActionResult GetPollById([FromRoute]int id)
        {
            Poll poll = _database.Polls.FirstOrDefault(p => id < p.Id);
            if (poll == null)
                return NotFound();
            JObject jsonObject = (JObject)JToken.FromObject(poll); // serializes C# object as a JObject
            jsonObject.Add("polling", new JArray()); // creates an array property on Jobject
            JArray optionsArray = ((JArray)jsonObject["polling"]); // gets a reference of the array
            foreach (PollOption pollOption in _database.PollOptions.Where( (p) => p.Poll.Id == poll.Id ) ){
                optionsArray.Add(new JObject(new JProperty(pollOption.PollOptionText, pollOption.PollOptionCount)) );
                // add JSON objects to the polling array to represent vote counting       Ex. => polling: [ {'option A': 2}, {'option B': 5} ] 
            }
            return Ok(jsonObject.ToString());
        }

        [HttpGet("active")]
        public IEnumerable<Poll> GetActivePolls() =>
            _database.Polls.Where(p => DateTime.Now < p.CloseDate);

        [HttpGet("inactive")]
        public IEnumerable<Poll> GetInactivePolls() =>
            _database.Polls.Where(p => p.CloseDate < DateTime.Now);

        [HttpPost("create")]
        public IActionResult CreatePoll([FromQuery] string question, 
                                        [FromQuery] bool canMultiOption,
                                        //[FromQuery] bool canEditVote,
                                        [FromQuery] DateTime? limitDate, 
                                        [FromBody] ICollection<String> options ) //2020-02-02T22:15:57Z
        {
            DateTime now = DateTime.Now; // Make sure to use the same instant of time to compute all validations

            // Check if there is another active poll with the same name
            if (_database.Polls.FirstOrDefault(p => p.QuestionText == question && p.CloseDate < DateTime.Now) == null) 
            {
                return BadRequest("There is already a poll with that alias. Try another name.");
            }

            // Check if request sent at least 2 options
            if (options.Count < 2 || options.Count > 30)
            {
                return BadRequest("A poll should have from two (2) to thirty (30) options.");
            }

            limitDate ??= now.AddDays(30); // default limit date to 30 days after created

            // Check minimum date
            if (limitDate <= now.AddMinutes(1))
            {
                return BadRequest("Poll should have a minimum 1 minute duration.");
            }
            // Check maximum date
            else if (limitDate > now.AddDays(30)) 
            {
                return BadRequest("Poll should have a maximum 30 days duration.");
            }

            Poll p = new Poll
            {
                QuestionText = question,
                //CanEditVote = canEditVote,
                CanMultiOption = canMultiOption,
                CloseDate = (DateTime)limitDate 
            };

            _database.Polls.Add(p);
            _database.SaveChanges();

            int i = 1;
            foreach (var optext in options) 
            {
                _database.PollOptions.Add(
                    new PollOption()
                    {
                        Poll = p, // foreign key reference
                        PollOptionText = optext,
                        PollOptionId = i
                    }
                 );
                i++;
            }
            _database.SaveChanges();
            Response.ContentType = "application/json; charset=utf-8";
            return new JsonResult(p);
        }
    }
}
