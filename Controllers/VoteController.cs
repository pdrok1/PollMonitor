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
    [Route("api/[controller]")]
    public class VoteController : ControllerBase
    {

        private readonly ApplicationDbContext _database;

        public VoteController(ApplicationDbContext database) 
        {
            _database = database;
        }

        [HttpPost("[id]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Vote([FromRoute]long id, [FromQuery] IDictionary<string, bool> options)
        {
            Poll poll = _database.Polls.FirstOrDefault((p) => p.Id == id);
            if (poll == null)
            {
                return BadRequest();
            }

            string teste = "";
            foreach(var option in options)
                teste += option.Key + "=" + option.Value + "&";
            return new JsonResult(teste+"id="+id.ToString());
        }
    }
}
