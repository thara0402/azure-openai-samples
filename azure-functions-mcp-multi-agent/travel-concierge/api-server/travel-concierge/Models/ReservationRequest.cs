using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace travel_concierge.Models
{
    public class ReservationRequest
    {
        [Description("行き先のホテルの名前。")]
        public required string Destination { get; set; }
        [Description("チェックイン日。YYYY/MM/DD形式。")]
        public required string CheckIn { get; set; }
        [Description("チェックアウト日。YYYY/MM/DD形式。")]
        public required string CheckOut { get; set; }
        [Description("宿泊人数。")]
        public required string GuestsCount { get; set; }
    }
}
