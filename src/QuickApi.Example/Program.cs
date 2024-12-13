using QuickApi.Engine.Web;
using QuickApi.Engine.Web.Cqrs;
using QuickApi.Example.Cqrs;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddMinimalEndpoints();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddScoped<IMessage, MessageService>();

var app = builder.Build();
app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();
app.UseMinimalEndpoints();
app.Run();