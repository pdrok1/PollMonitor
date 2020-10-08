using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PollMonitor.Models
{
    public abstract class AbstractModel
    {
        [Required]
        [Column("RegisterDate")]
        [Display(Name = "Register Date")]
        public DateTime RegisterDate { get; } = DateTime.Now;
    }
}
