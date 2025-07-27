using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace travel_concierge.Agents
{
    public class ReservationAgent(ILogger<ReservationAgent> logger)
    {
        private readonly ILogger<ReservationAgent> _logger = logger;

        [Function(nameof(SubmitReservation))]
        public string SubmitReservation(
            [McpToolTrigger("submit_reservation", "宿泊先の予約を行います。")] ToolInvocationContext context,
            [McpToolProperty("destination", "string", "行き先のホテルの名前。")] string destination,
            [McpToolProperty("checkIn", "string", "チェックイン日。YYYY/MM/DD形式。")] string checkIn,
            [McpToolProperty("checkOut", "string", "チェックアウト日。YYYY/MM/DD形式。")] string checkOut,
            [McpToolProperty("guestsCount", "string", "宿泊人数。")] string guestsCount)
        {
            // This is sample code. Replace this with your own logic.
            var result = $"""
            予約番号は {Guid.NewGuid()} です。
            --------------------------------
            ホテル名：{destination}
            チェックイン日：{checkIn}
            チェックアウト日：{checkOut}
            人数：{guestsCount} 名
            --------------------------------
            """;

            return result;
        }
    }
}