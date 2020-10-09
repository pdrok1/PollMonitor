using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PollMonitor.Repository;
using PollMonitor.Models;

namespace PollMonitor.Controllers
{
    [ApiController]
    [Route("api/[controller]/")]
    public class VoteController : ControllerBase
    {

        private readonly ApplicationDbContext _database;

        public VoteController(ApplicationDbContext database) // SQLServer EntityFramework service DI
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

            // check if poll is inactive
            if (poll.CloseDate < DateTime.Now)
                // redirect to poll options and results if it is closed
                return RedirectPermanent("/api/poll/" + id.ToString());

            // check if already voted on this poll
            Vote vote = _database.Votes.FirstOrDefault( (v) =>
                 v.Poll.Id == id && v.OriginIp.Equals(Request.HttpContext.Connection.RemoteIpAddress.ToString()));
            if (vote != null)
                return BadRequest("Your vote for this poll is already computed.");

            // validate if surpasses poll.SelectableOptionsCount limits
            if (options.Count < 1)
                return BadRequest("A vote should have at least one option selected");

            if ( options.Count > poll.SelectableOptionsCount )
                return BadRequest($"This poll is set to receive at maximum {poll.SelectableOptionsCount} selected option" + (poll.SelectableOptionsCount > 1?"s":"") + ".");

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

            // log user selected options
            vote = new Vote()
            {
                Poll = poll,
                OriginIp = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                PollOptions = GetBitwiseOptionByteArray(options)
            };
            _database.Votes.Add(vote);
            _database.SaveChanges();

            return Ok("Vote computed.");
        }

        private int GetBitwiseOptionByteArray(IDictionary<int,bool> ops) 
        {
            int selectedOptions = 0;
            foreach (var pair in ops) 
            {
                if(pair.Value)
                    selectedOptions |= 2 ^ (pair.Key - 1);
                /* 
                 * It writes inside the Int32 bits the choosen options in its relative position, so Ex.: if I voted options 1, 2 and 5, assuming
                 * the machines processor is Big-Endian, it would write 
                 * 0000 0000  0000 0000  0000 0000  0000 0001 = 2 ^ (1 - 1) 
                 *                 logical or
                 * 0000 0000  0000 0000  0000 0000  0000 0010 = 2 ^ (2 - 1)
                 *                 logical or
                 * 0000 0000  0000 0000  0000 0000  0001 0000 = 2 ^ (5 - 1)
                 *                     =
                 * 0000 0000  0000 0000  0000 0000  0001 0011 = 1 + 2 + 16 = 19
                 * I think it's better for performance to actually change each bit instead of using power functions, but I couldn't find a safer 
                 * way to control computer Endianness.
                */
            }
            return selectedOptions;
        }
    }
}
