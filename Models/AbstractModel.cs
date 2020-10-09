using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PollMonitor.Models
{
    public class AbstractModel
    {
        public DateTime RegisterDate { get; set; } = DateTime.Now;
    }
}
