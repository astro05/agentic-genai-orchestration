using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TextGeneration.Models
{
    public class JobDetails
    {
        public string Title { get; set; }
        public JobLevel Level { get; set; }
        public JobType Type { get; set; }
        public int MinSalary { get; set; }
        public int MaxSalary { get; set; }
        public string[] Skills { get; set; }
        public string TenWordSummary { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum JobLevel
    {
        Junior,
        Mid,
        Senior
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum JobType
    {
        Remote,
        Onsite,
        Hybrid
    }
}
