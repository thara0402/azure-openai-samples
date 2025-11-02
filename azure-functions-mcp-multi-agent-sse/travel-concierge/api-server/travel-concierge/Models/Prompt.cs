using Microsoft.Extensions.AI;

namespace travel_concierge.Models
{
    public class Prompt
    {
        public List<PromptMessageItem> Messages { get; set; } = default!;
    }

    public class PromptMessageItem
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public static class PromptMessageItemExtension
    {
        public static IEnumerable<ChatMessage> ConvertToChatMessageArray(this IEnumerable<PromptMessageItem> messages)
        {
            return messages.Select(m =>
            {
                return m.Role switch
                {
                    "user" => new ChatMessage(ChatRole.User, m.Content),
                    "assistant" => new ChatMessage(ChatRole.Assistant, m.Content),
                    _ => throw new InvalidOperationException($"You can not set role: {m.Role}")
                };
            });
        }
    }
}
