﻿
namespace travel_concierge.Models
{
    public class SynthesizedResult
    {
        public string Content { get; set; } = string.Empty;
        public List<string> CalledAgentNames { get; set; } = new List<string>();
    }
}
