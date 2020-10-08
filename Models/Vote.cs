using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PollMonitor.Models
{
    public class Vote : AbstractModel
    {
        [Key]
        public long VoteId { get; set; }

        [Required, MinLength(7), MaxLength(40)]
        private string OriginIp { get; set; }
        [Required]
        public Poll Poll { get; set; }

        [NotMapped]
        public IPAddress OriginIpAddress
        {
            get
            {
                return IPAddress.Parse(this.OriginIp);
            }
            set
            {
                OriginIp = value.ToString();
            }
        }

        public int PollOptions { get; set; }
        // By business rule, I defined that a poll should have at maximum 30 options.
        // On polls with CanMultiOption enabled, the API can save bit-wise multiple selections by the same user on the PollOptions property.
    }
}
