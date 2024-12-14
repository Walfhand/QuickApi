using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickApi.Example.Features.Todos.Domain;

namespace QuickApi.Example.Features.Todos.Etcs;

public class TodoEtc : IEntityTypeConfiguration<Todo>
{
    public void Configure(EntityTypeBuilder<Todo> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new TodoId(value))
            .ValueGeneratedNever();
    }
}