﻿
namespace travel_concierge.Models
{
    public class AgentsToRun
    {
        public bool IsAgentCall { get; set; }
        public string Content { get; set; } = string.Empty;
        public IList<AgentCall> AgentCalls { get; set; } = default!;
    }
    
    public class AgentCall
    {
        public string AgentName { get; set; } = string.Empty;
        public object Arguments { get; set; } = string.Empty;
    }
}
