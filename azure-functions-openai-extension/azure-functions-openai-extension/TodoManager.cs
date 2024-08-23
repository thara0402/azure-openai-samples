using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace azure_functions_openai_extension
{
    public record TodoItem(string Id, string Task);

    public interface ITodoManager
    {
        Task AddTodoAsync(TodoItem todo);

        Task<IReadOnlyList<TodoItem>> GetTodosAsync();
    }

    class InMemoryTodoManager : ITodoManager
    {
        readonly List<TodoItem> todos = new();

        public Task AddTodoAsync(TodoItem todo)
        {
            this.todos.Add(todo);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<TodoItem>> GetTodosAsync()
        {
            return Task.FromResult<IReadOnlyList<TodoItem>>(this.todos.ToImmutableList());
        }
    }
}
