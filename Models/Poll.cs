using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PollMonitor.Models
{
    public class Poll : AbstractModel
    {
        [Key]
        public long Id { get; set; }

        [MaxLength(200)]
        public string QuestionText { get; set; }

        [Required]
        public int SelectableOptionsCount { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public DateTime CloseDate { get; set; } // It is open when value is null on data source
        //Can be also used as a limit date

        public string CreatorUserIp { get; set; }

    }
}
