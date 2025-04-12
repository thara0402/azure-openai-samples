using System.ComponentModel;

namespace travel_concierge.Models
{
    public class GetDestinationSuggestParameter
    {
        [Description("行き先に求める希望の条件")]
        public required string SearchTerm { get; set; } = string.Empty;
    }
}
