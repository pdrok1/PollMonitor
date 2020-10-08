using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PollMonitor.Models
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class Poll : AbstractModel
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MinLength(5), MaxLength(200)]
        public string QuestionText { get; set; }

        [Required]
        [Range(1,30)]
        public int SelectableOptionsCount { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public DateTime CloseDate { get; set; } // determines whether Poll is open or closed

        public string CreatorUserIp { get; set; }

    }
}
