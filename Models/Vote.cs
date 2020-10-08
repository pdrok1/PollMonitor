using System.ComponentModel.DataAnnotations;

namespace PollMonitor.Models
{
    public class Vote : AbstractModel
    {
        [Key]
        public long Id { get; set; }

        [Required, MinLength(7), MaxLength(40)]
        public string OriginIp { get; set; }

        [Required]
        public Poll Poll { get; set; }

        [Required]
        public int PollOptions { get; set; }
        // By business rule, I defined that a poll should have at maximum 30 options.
        // On polls with CanMultiOption enabled, the API can save bit-wise multiple selections by the same user on the PollOptions property.
    }
}
