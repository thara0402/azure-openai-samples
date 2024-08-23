using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenAI.Assistants;
using Microsoft.Extensions.Logging;

namespace azure_functions_openai_extension
{
    public class AssistantSkills
    {
        private readonly ITodoManager _todoManager;
        private readonly ILogger<AssistantSkills> _logger;

        public AssistantSkills(ITodoManager todoManager, ILogger<AssistantSkills> logger)
        {
            _todoManager = todoManager;
            _logger = logger;
        }

        [Function(nameof(AddTodo))]
        public Task AddTodo([AssistantSkillTrigger("Create a new todo task")] string taskDescription)
        {
            if (string.IsNullOrEmpty(taskDescription))
            {
                throw new ArgumentException("Task description cannot be empty");
            }

            _logger.LogInformation("Adding todo: {task}", taskDescription);

            string todoId = Guid.NewGuid().ToString()[..6];
            return _todoManager.AddTodoAsync(new TodoItem(todoId, taskDescription));
        }

        [Function(nameof(GetTodos))]
        public Task<IReadOnlyList<TodoItem>> GetTodos(
            [AssistantSkillTrigger("Fetch the list of previously created todo tasks")] object inputIgnored)
        {
            _logger.LogInformation("Fetching list of todos");

            return _todoManager.GetTodosAsync();
        }
    }
}
