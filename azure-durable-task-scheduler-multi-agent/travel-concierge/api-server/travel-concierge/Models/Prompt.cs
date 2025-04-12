using OpenAI.Chat;

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
            return messages.Select<PromptMessageItem, ChatMessage>(m =>
            {
                return m.Role switch
                {
                    "user" => new UserChatMessage(m.Content),
                    "assistant" => new AssistantChatMessage(m.Content),
                    _ => throw new InvalidOperationException($"You can not set role: {m.Role}")
                };
            });
        }
    }
}
