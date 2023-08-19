using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aoai_function_calling_console_app
{
    internal class ApiClient
    {
        public static string SearchAutumnLeaves(string location)
        {
            // 本当は location を使って検索するが、今回は京都の名所を決め打ちで返す。
            return "{\n  \"result\": \"清水寺\",\n}";
        }
    }

    internal class AutumnLeavesParameter
    {
        public required string Location { get; set; }
    }
}
