using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.WebEncoders.Testing;
using PollMonitor.Repository;
using PollMonitor.Models;

namespace PollMonitor.Controllers
{
    [ApiController]
    [Route("api/[controller]/")]
    public class VoteController : ControllerBase
    {

        private readonly ApplicationDbContext _database;

        public VoteController(ApplicationDbContext database) // SQLServer service DI
        {
            _database = database;
        }

        [HttpPost("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Vote([FromRoute]long id, [FromQuery] IDictionary<int, bool> options)
        {
            // check if poll exists
            Poll poll = _database.Polls.Find(id);
            if (poll == null)
                return NotFound();

            // check if already voted on this poll
            Vote vote = _database.Votes.FirstOrDefault( (v) =>
                 v.Poll.Id == id && v.OriginIpAddress.Equals(Request.HttpContext.Connection.RemoteIpAddress));
            if (vote != null)
                return BadRequest("Your vote for this poll is already computed.");

            // validate if surpasses poll.SelectableOptionsCount limit
            if ( options.Count > poll.SelectableOptionsCount )
                return BadRequest($"This poll is set to receive at maximum {poll.SelectableOptionsCount} selected option" + (poll.SelectableOptionsCount > 1?"s":"") + ".");

            if (options.Count < 1)
                return BadRequest("A vote should have at least one option selected");
            // validate poll options caught on request
            var choosableOptions = _database.PollOptions.Where( (po) => po.Poll.Id == id );
            foreach (var o in options)
            {
                // if any of the request keys are wrong, respond BadRequest
                var pollOption = choosableOptions.FirstOrDefault((co) => co.PollOptionId == o.Key);
                if (pollOption == null) 
                    return BadRequest("Some request poll option is invalid.");
                pollOption.PollOptionVoteCount++;
                _database.PollOptions.Update(pollOption);
            }
            _database.SaveChanges();

            // log user selected options

            vote = new Vote()
            {
                Poll = poll,
                OriginIpAddress = Request.HttpContext.Connection.RemoteIpAddress,
                PollOptions = GetBitwiseOptionByteArray(options)
            };
            _database.Votes.Add(vote);

            return Ok("Vote computed.");
        }

        private int GetBitwiseOptionByteArray(IDictionary<int,bool> ops) 
        {
            int selectedOptions = 0;
            foreach (var pair in ops) 
            {
                if(pair.Value)
                    selectedOptions |= 2 ^ pair.Key;
            }
            return selectedOptions;
        }
    }
}
