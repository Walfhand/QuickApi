namespace QuickApi.Example.Features.Domain;

public class Todo
{
    private Todo(string title, string? description)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        IsCompleted = false;
    }
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; set; }
    public bool IsCompleted { get; private set; }


    public static Todo Create(string title, string? description)
    {
        return new Todo(title, description);
    }
}