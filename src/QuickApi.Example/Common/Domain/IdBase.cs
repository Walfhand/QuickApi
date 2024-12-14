namespace QuickApi.Example.Common.Domain;

public record IdBase(Guid Value)
{
    public IdBase() : this(Guid.NewGuid())
    {
        
    }

    public static implicit operator Guid(IdBase id) => id.Value;
    public static implicit operator IdBase(Guid id) => new(id);
}