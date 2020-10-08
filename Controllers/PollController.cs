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
    
    [ApiController]
    [Route("api/[controller]/")]
    public class PollController : ControllerBase
    {
        private readonly ApplicationDbContext _database;
        public PollController(ApplicationDbContext database)
        {
            this._database = database;
        }

        [HttpGet("{id}")]
        public IActionResult GetPollById([FromRoute]long id)
        {
            Poll poll = _database.Polls.Find(id);
            if (poll == null)
                return NotFound();
            JObject jsonObject = (JObject)JToken.FromObject(poll); // serializes C# object as a JObject
            jsonObject.Add("polling", new JArray()); // creates an array property on jsonObject
            JArray optionsArray = ((JArray)jsonObject["polling"]); // gets a reference of the array

            // the first element counts how many diferent users voted this poll
            // now the first item will actually start on index 1, paired with the options OrderBy'ed in the foreach
            optionsArray.Add(new JObject(new JProperty("_voteUsersCount", _database.Votes.Where( (v) => v.Poll.Id == poll.Id).Count())));

            foreach (PollOption pollOption in _database.PollOptions.Where( (p) => p.Poll.Id == poll.Id ).OrderBy( (po) => po.PollOptionId ) )
                optionsArray.Add(new JObject(new JProperty(pollOption.PollOptionText, pollOption.PollOptionVoteCount)) );
                // add JSON objects to the polling array to vote counting       Ex. => polling: [ {'Make thing A': 2}, {'Make thing B': 5} ] 
            
            return Ok(jsonObject.ToString());
        }

        [HttpGet("active")]
        public IEnumerable<Poll> GetActivePolls() =>
            _database.Polls.Where(p => DateTime.Now < p.CloseDate);

        [HttpGet("inactive")]
        public IEnumerable<Poll> GetInactivePolls() =>
            _database.Polls.Where(p => p.CloseDate < DateTime.Now);

        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreatePoll([FromQuery] string question, 
                                        [FromQuery] int selectableOptionsCount,
                                        [FromQuery] DateTime? limitDate, 
                                        [FromBody] ICollection<String> options ) //2020-02-02T22:15:57Z
        {
            try
            {
                DateTime now = DateTime.Now; // Make sure to use the same instant of time to compute all validations

                // Check if there is another active poll with the same name
                if (_database.Polls.FirstOrDefault(p => p.QuestionText == question && p.CloseDate < DateTime.Now) == null)
                    return BadRequest("There is already an active poll with that alias. Try another question.");

                // validate selectableOptionsCount (at least 1 option should be selected on voting)
                if (selectableOptionsCount < 1)
                    return BadRequest("Invalid argument selectableOptionsCount, must be one (1) or greater.");

                // check if request sent at least 2 options and at max 30 options
                if (options.Count < 2 || options.Count > 30)
                    return BadRequest("A poll should have from two (2) to thirty (30) options.");

                limitDate ??= now.AddDays(30); // default limit date to 30 days after created

                // check minimum date
                if (limitDate <= now.AddMinutes(1))
                    return BadRequest("Poll should have a minimum 1 minute duration.");

                // check maximum date
                else if (limitDate > now.AddDays(30))
                    return BadRequest("Poll should have a maximum 30 days duration.");

                Poll p = new Poll
                {
                    QuestionText = question,
                    SelectableOptionsCount = selectableOptionsCount,
                    CloseDate = (DateTime)limitDate
                };
                _database.Polls.Add(p);

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
            catch (Exception e) 
            {
                return BadRequest("Sei lá");
            }
        }
    }
}
