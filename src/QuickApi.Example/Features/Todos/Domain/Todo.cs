using QuickApi.Example.Common.Domain;

namespace QuickApi.Example.Features.Todos.Domain;


public record TodoId : IdBase
{
    public TodoId(Guid value) : base(value)
    {
        
    }

    public TodoId()
    {
        
    }
}

public class Todo
{
    private Todo(string title, string? description)
    {
        Id = new TodoId();
        Title = title;
        Description = description;
        IsCompleted = false;
    }
    public TodoId Id { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public bool IsCompleted { get; private set; }


    public static Todo Create(string title, string? description)
    {
        return new Todo(title, description);
    }

    public void Complete()
    {
        IsCompleted = true;
    }

    public void Update(string title, string? description)
    {
        Title = title;
        Description = description;
    }
}