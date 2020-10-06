using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using PollMonitor.Repository;
using System.Linq;

namespace PollMonitor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PollController : ControllerBase
    {
        private readonly ApplicationDbContext _database;
        public PollController(ApplicationDbContext database)
        {
            this._database = database;
        }

        [HttpGet]
        public IActionResult getPolls() 
        {
            //
            string r = "{ polls: [ " +  + " ] } ";
            Response.ContentType = "application/json; charset=utf-8";
            return new JsonResult(r);
        }
    }
}
