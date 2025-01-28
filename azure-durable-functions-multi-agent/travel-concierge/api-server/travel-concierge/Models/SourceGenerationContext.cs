using System.Text.Json.Serialization;

namespace travel_concierge.Models
{
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(GetClimateParameter))]
    [JsonSerializable(typeof(GetDestinationSuggestParameter))]
    [JsonSerializable(typeof(GetHotelParameter))]
    [JsonSerializable(typeof(GetSightseeingSpotParameter))]
    [JsonSerializable(typeof(SubmitReservationParameter))]
    internal partial class SourceGenerationContext : JsonSerializerContext;
}
