using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PollMonitor.Models
{
    public class Vote : AbstractModel
    {
        [Key]
        public string OriginIp { get; set; }
        [Key]
        public Poll Poll { get; set; }

        public int PollOption { get; set; }
        // By business rule, I defined that a poll should have at maximum 30 options.
        // On polls with CanMultiOption enabled, the API can save bit-wise multiple selections by the same user on the PollOption property.
    }
}
