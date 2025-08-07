using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using QuickApi.Engine.Web;
using QuickApi.Example.Config.Cors;
using QuickApi.Example.Data.Configs;
using QuickApi.Extensions.Mediatr;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMinimalEndpoints(options =>
{
    options.AddMediatR();
    options.SetBaseApiPath("api/v1");
});
builder.Services.AddCustomCors();
builder.Services.AddHttpContextAccessor();
builder.Services.AddData();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    var proxyAddresses = Dns.GetHostAddresses("caddy");

    foreach (var address in proxyAddresses)
    {
        options.KnownProxies.Add(address);
    }
});
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        var httpContextAccessor = context.ApplicationServices.GetService<IHttpContextAccessor>();
        var httpContext = httpContextAccessor?.HttpContext;

        if (httpContext == null) return Task.CompletedTask;
        var host = httpContext.Request.Host.ToString();
        var scheme = httpContext.Request.Scheme;
        var baseUrl = $"{scheme}://{host}";
        document.Servers.Clear();
        document.Servers.Add(new OpenApiServer
        {
            Url = baseUrl,
            Description = "Base URL déterminée dynamiquement"
        });

        return Task.CompletedTask;
    });
});

var app = builder.Build();
app.UseData();
app.UseForwardedHeaders();
app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();
app.UseMinimalEndpoints();
app.UseCustomCors();
app.Run();