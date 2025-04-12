
namespace travel_concierge.Models
{
    public class AgentCallResults
    {
        public List<string> Results { get; set; } = new List<string>();
        public Prompt Prompt { get; set; } = new Prompt();
        public List<string> CalledAgentNames { get; set; } = new List<string>();
    }
}
