using Microsoft.Extensions.Options;
using pdf_summarizer.Models;

namespace pdf_summarizer.Agents
{
    internal abstract class AgentBase
    {
        protected readonly MySettings _settings;

        protected AgentBase(IOptions<MySettings> optionsAccessor)
        {
            _settings = optionsAccessor.Value;
        }
    }
}
