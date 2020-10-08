using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PollMonitor.Models
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public abstract class AbstractModel
    {
        public AbstractModel() {
            DateTime now = DateTime.Now;
            this.RegisterDate = now;
        }
        [Required]
        [Column("RegisterDate")]
        [Display(Name = "Register Date")]
        public DateTime RegisterDate { get; }
    }
}
