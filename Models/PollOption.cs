using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PollMonitor.Models
{
    public class PollOption : AbstractModel
    {
        [Index]
        [Key, Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Poll Poll { get; set; }

        [Index]
        [Key, Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long PollOptionId { get; set; }

        [Required]
        public string PollOptionText { get; set; }

        [Required]
        public int PollOptionVoteCount { get; set; } = 0;
    }
}
