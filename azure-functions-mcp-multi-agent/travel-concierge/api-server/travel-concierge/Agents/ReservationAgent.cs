using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;
using travel_concierge.Models;

namespace travel_concierge.Agents
{
    public class ReservationAgent(ILogger<ReservationAgent> logger)
    {
        private readonly ILogger<ReservationAgent> _logger = logger;

        //[Function(nameof(SubmitReservation))]
        //public string SubmitReservation(
        //    [McpToolTrigger("submit_reservation", "宿泊先の予約を行います。")] ToolInvocationContext context,
        //    [McpToolProperty("destination", "行き先のホテルの名前。", true)] string destination,
        //    [McpToolProperty("checkIn", "チェックイン日。YYYY/MM/DD形式。", true)] string checkIn,
        //    [McpToolProperty("checkOut", "チェックアウト日。YYYY/MM/DD形式。", true)] string checkOut,
        //    [McpToolProperty("guestsCount", "宿泊人数。", true)] string guestsCount)
        //{
        //    // This is sample code. Replace this with your own logic.
        //    var result = $"""
        //    予約番号は {Guid.NewGuid()} です。
        //    --------------------------------
        //    ホテル名：{destination}
        //    チェックイン日：{checkIn}
        //    チェックアウト日：{checkOut}
        //    人数：{guestsCount} 名
        //    --------------------------------
        //    """;

        //    return result;
        //}

        [Function(nameof(SubmitReservation))]
        public string SubmitReservation(
            [McpToolTrigger(nameof(SubmitReservation), "宿泊先の予約を行います。")] ReservationRequest request, ToolInvocationContext context)
        {
            // This is sample code. Replace this with your own logic.
            var result = $"""
            予約番号は {Guid.NewGuid()} です。
            --------------------------------
            ホテル名：{request.Destination}
            チェックイン日：{request.CheckIn}
            チェックアウト日：{request.CheckOut}
            人数：{request.GuestsCount} 名
            --------------------------------
            """;

            return result;
        }
    }
}