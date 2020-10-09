using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PollMonitor.Repository;
using PollMonitor.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;

namespace PollMonitor.Controllers
{

    [ApiController]
    [Route("api/[controller]/")]
    public class PollController : ControllerBase
    {
        private readonly ApplicationDbContext _database;
        public PollController(ApplicationDbContext database) // SQLServer EntityFramework service DI
        {
            this._database = database;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public IActionResult GetPollById([FromRoute] long id)
        {
            try
            {
                Poll poll = _database.Polls.Find(id);
                if (poll == null)
                    return NotFound();
                return new ContentResult { Content = GetPollJsonWithOptions(poll).ToString(Formatting.None), ContentType = "application/json" };
            }
            catch (Exception e) 
            {
                return new ContentResult { Content = e.StackTrace.ToString() };
            }
        }


        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("active")]
        public IEnumerable<Poll> GetActivePolls() => GetPolls( (dtObject) => ( (p) =>  dtObject < p.CloseDate) );


        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("inactive")]
        public IEnumerable<Poll> GetInactivePolls() => GetPolls( (dtObject) => ( (p) => p.CloseDate < dtObject) );


        public IQueryable<Poll> GetPolls(Func<DateTime, System.Linq.Expressions.Expression<Func<Poll, bool>>> predicate)
        {
            DateTime now = DateTime.Now;
            /* 
             * for some reason, a DateTime.Now directly passed as a predicate messes with the time zone (I don't know why)
             * so I need to pass a fixed variable
            */
            return _database.Polls.Where(predicate(now));
        }

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
                // Make sure to use the same instant of time to compute all validations
                DateTime now = DateTime.Now;

                // Validate question string size
                if(question.Length < 5)
                    return BadRequest("Question text too little, request refused.");

                // Check if there is another active poll with the same name
                if (_database.Polls.FirstOrDefault(p => p.QuestionText == question && now < p.CloseDate) != null)
                    return BadRequest("There is already an active poll with that alias. Try another question.");

                // validate selectableOptionsCount (at least 1 option should be selected on voting)
                if (selectableOptionsCount < 1)
                    return BadRequest("Invalid argument selectableOptionsCount, must be one (1) or greater.");

                // check if request sent at least 2 options and at max 30 options
                if (options.Count < 2 || options.Count > 30)
                    return BadRequest("A poll should have from two (2) to thirty (30) options.");

                limitDate ??= now.AddDays(30); // default limit date to 30 days after created

                // check if sent an already outdated close date
                if (limitDate <= now)
                    return BadRequest($"Outdated close date {limitDate}. Server Timezone is {TimeZoneInfo.Local}.");

                // check minimum date
                if (limitDate <= now.AddMinutes(1))
                    return BadRequest($"Poll should have a minimum 1 minute duration. Date sent: {limitDate}. Make sure it is timezone +0.");

                // check maximum date
                else if (limitDate > now.AddDays(30))
                    return BadRequest($"Poll should have a maximum 30 days duration. Date sent: {limitDate}. Make sure it is timezone +0.");

                Poll p = new Poll
                {
                    QuestionText = question,
                    SelectableOptionsCount = selectableOptionsCount,
                    CloseDate = (DateTime)limitDate,
                    CreatorUserIp = Request.HttpContext.Connection.RemoteIpAddress.ToString()
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
                return new ContentResult { Content = GetPollJsonWithOptions(p).ToString(Formatting.None), ContentType = "application/json" };
            }
            catch (DbUpdateException due)
            {
                return BadRequest("Error updating data to data source. Check your request parameters.\nStack trace: " + due.StackTrace.ToString());
            }
            catch (Exception e)
            {
                return BadRequest("Error from server. Check your request parameters.\nStack trace: " + e.StackTrace.ToString());
            }
        }


        private JObject GetPollJsonWithOptions(Poll poll) 
        {
            JObject jsonObject = (JObject)JToken.FromObject(poll); // serializes C# object as a JObject
            jsonObject.Add("polling", new JArray()); // creates an array property on jsonObject
            JArray optionsArray = ((JArray)jsonObject["polling"]); // gets a reference of the array

            // the first element counts how many diferent users voted this poll
            // now the first item will actually start on index 1 of the array, paired with the options OrderBy'ed in the foreach below
            optionsArray.Add(new JObject(new JProperty("_voteUsersCount", _database.Votes.Where((v) => v.Poll.Id == poll.Id).Count())));

            foreach (PollOption pollOption in _database.PollOptions.Where((p) => p.Poll.Id == poll.Id).OrderBy((po) => po.PollOptionId))
                optionsArray.Add(new JObject(new JProperty(pollOption.PollOptionText, pollOption.PollOptionVoteCount)));
            // add JSON objects to the polling array to vote counting       Ex. => polling: [ {'_voteUsersCount' : 6}, {'Make thing A': 2}, {'Make thing B': 5} ] 
            return jsonObject;
        }
    }
}
