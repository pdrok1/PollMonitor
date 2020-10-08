using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PollMonitor.Models
{
    public class PollOption : AbstractModel
    {
        [Index]
        [Key]
        public long Id { get; set; } // Entity identity field

        [Index]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Poll Poll { get; set; } // Foreign key

        
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        [Range(1,30)]
        public int PollOptionId { get; set; } // Poll Option indentifier

        [Required]
        public string PollOptionText { get; set; }

        [Required]
        public int PollOptionVoteCount { get; set; } = 0;
    }
}
