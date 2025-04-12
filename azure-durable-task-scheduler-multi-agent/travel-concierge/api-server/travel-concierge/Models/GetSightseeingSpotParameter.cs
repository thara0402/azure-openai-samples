﻿using System.ComponentModel;

namespace travel_concierge.Models
{
    public class GetSightseeingSpotParameter
    {
        [Description("場所の名前。例: ボストン, 東京、フランス")]
        public required string Location { get; set; } = string.Empty;
    }
}
