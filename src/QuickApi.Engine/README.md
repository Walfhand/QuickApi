# QuickApi.Engine

[![NuGet](https://img.shields.io/nuget/v/Walfhand.QuickApi.svg)](https://www.nuget.org/packages/Walfhand.QuickApi)
[![Build Status](https://github.com/Walfhand/QuickApi/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/Walfhand/QuickApi/actions)

## Overview

QuickApi is a lightweight library designed to simplify the development of Minimal APIs in .NET applications. It provides seamless integration with popular CQRS tools like MediatR and Wolverine, enabling developers to quickly set up and scale their APIs with minimal boilerplate code.

## Features

- **Effortless Minimal API Setup**: Simplifies configuration and reduces the boilerplate code.
- **Automatic Endpoint Discovery**: Scans loaded assemblies and registers every non-abstract `IMinimalEndpoint` with a configurable lifetime.
- **CQRS Integration**: Built-in support for MediatR and Wolverine for handling commands, queries, and events.
- **Configurable API Prefix & Tags**: Routes are automatically prefixed (default `api`) and tagged using the first path segment for better OpenAPI grouping.
- **Response Conventions**: Base endpoints wire sensible `Produces(...)`/`ProducesProblem(...)` metadata (e.g., 201 on POST, 204 on PUT/PATCH/DELETE, 404 on GET).
- **Paginated & File Helpers**: `PaginatedResult<T>` and `FileResult` helpers for list and file endpoints.
- **Developer Friendly**: Focuses on improving productivity and code readability.
- **NuGet Package**: Easily installable via NuGet.

## Installation

QuickApi is available on [NuGet](https://www.nuget.org/packages/Walfhand.QuickApi/). Install the base package:

```bash
dotnet add package Walfhand.QuickApi
```

For MediatR integration, you'll also need to install:

```bash
dotnet add package Walfhand.QuickApi.Extensions.Mediatr
```

## Getting Started

### Step 1: Installation

Install the required packages using the .NET CLI as shown in the Installation section above.

### Step 2: Register QuickApi in your Program.cs

Add the following lines to your `Program.cs` file:

```csharp
// Basic setup
builder.Services.AddMinimalEndpoints();

// Or with MediatR integration
builder.Services.AddMinimalEndpoints(options =>
{
    options.AddMediatR();
});

app.UseMinimalEndpoints();
```

To change the global API prefix (defaults to `api`):

```csharp
builder.Services.AddMinimalEndpoints(options =>
{
    options.SetBaseApiPath("api/v1");
});
```
All endpoints will then be exposed as `/api/v1/<your-path>`.

### Step 3: Implement the IMessage Interface

If you're using MediatR integration (configured via `options.AddMediatR()`), you can skip this step as the implementation below is already provided for you.

For custom CQRS implementations, implement the `IMessage` interface. Here is an example of how the MediatR implementation looks like internally:

```csharp
using MediatR;
using QuickApi.Engine.Web.Cqrs;

namespace QuickApi.Example.Cqrs;

// Note: This implementation is provided automatically when using options.AddMediatR()
// Only implement this if you're NOT using AddMediatR() or if you need a custom implementation
public class MessageService : IMessage
{
    private readonly IMediator _mediator;

    public MessageService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<TResult> InvokeAsync<TResult>(object message, CancellationToken ct = default)
    {
        return await _mediator.Send((IRequest<TResult>)message, ct);
    }

    public async Task InvokeAsync(object message, CancellationToken ct = default)
    {
        await _mediator.Send(message, ct);
    }
}
```

### Step 4: Register the Message Service

If you're using MediatR integration (configured via `options.AddMediatR()`), you can skip this step.

For custom CQRS implementations, register your `MessageService` implementation in the `Program.cs` file:

```csharp
builder.Services.AddScoped<IMessage, MessageService>();
```

### Configuration Notes (MinimalApiOptions)

- **Endpoint lifetime**: defaults to `Scoped`. Override with `options.ServiceLifetime = ServiceLifetime.Transient;` (or `Singleton`) when registering endpoints.
- **Additional service wiring**: use `options.AddConfiguration(...)` to plug extra registrations alongside QuickApi (the MediatR extension uses this internally):

```csharp
builder.Services.AddMinimalEndpoints(options =>
{
    options.AddConfiguration(services =>
    {
        services.AddSingleton<IMyDependency, MyDependency>();
    });
});
```
- **Route prefix & tags**: routes are automatically prefixed with the configured base path and tagged using the first segment of the route (e.g., `todos`) for cleaner Swagger grouping.

### Step 5: Adding Endpoints

With QuickApi, you can define endpoints anywhere in your code by implementing one of the provided endpoint base classes. `AddMinimalEndpoints` automatically discovers every non-abstract implementation of `IMinimalEndpoint` in the loaded assemblies and maps them with the configured lifetime.

#### Example: Adding a Todo Item without Policies

The following example uses MediatR's CQRS implementation (`IRequest` and `IRequestHandler`). The specific interfaces will vary depending on your chosen CQRS framework:

```csharp
using MediatR;
using QuickApi.Engine.Web.Endpoints.Impl;
using QuickApi.Example.Features.Todos.Domain;

namespace QuickApi.Example.Features.Todos.AddTodo.Endpoints;

// MediatR specific: IRequest<Todo> (command)
public record AddTodoRequest(string Title, string? Description) : IRequest<Todo>;

public class AddTodoEndpoint() : PostMinimalEndpoint<AddTodoRequest, Todo>("todos")
{
}

// MediatR specific: IRequestHandler<AddTodoRequest, Todo>
public class AddTodoRequestHandler : IRequestHandler<AddTodoRequest, Todo>
{
    public Task<Todo> Handle(AddTodoRequest request, CancellationToken cancellationToken)
    {
        // Example logic for creating a Todo item
        var todo = new Todo { Title = request.Title, Description = request.Description };
        return Task.FromResult(todo);
    }
}
```

#### Example: Adding a Todo Item with Policies

Similar to the previous example, this uses MediatR's CQRS implementation:

```csharp
using MediatR;
using QuickApi.Engine.Web.Endpoints.Impl;
using QuickApi.Example.Features.Todos.Domain;

namespace QuickApi.Example.Features.Todos.AddTodo.Endpoints;

// MediatR specific: IRequest<Todo> (command)
public record AddTodoRequest(string Title, string? Description) : IRequest<Todo>;

public class AddTodoEndpoint() : PostMinimalEndpoint<AddTodoRequest, Todo>(
    "todos",
    nameof(PoliciesEnum.Subscribed))
{
}

// MediatR specific: IRequestHandler<AddTodoRequest, Todo>
public class AddTodoRequestHandler : IRequestHandler<AddTodoRequest, Todo>
{
    public Task<Todo> Handle(AddTodoRequest request, CancellationToken cancellationToken)
    {
        // Example logic for creating a Todo item
        var todo = new Todo { Title = request.Title, Description = request.Description };
        return Task.FromResult(todo);
    }
}

public class PoliciesEnum
{
    public const string Subscribed = "Subscribed";
}
```

#### Example: Customizing an Endpoint without MediatR

You can bypass the `IMessage` interface and MediatR by customizing your endpoint directly:

```csharp
public class FilterTodoEndpointCustom() : FilterMinimalEndpoint<FilterTodoRequest, Todo>("todos/custom")
{
    protected override RouteHandlerBuilder Configure(IEndpointRouteBuilder builder)
    {
        //call base configure and get routeHandlerBuilder
        var routeHandlerBuilder = base.Configure(builder);

        //add your customization
        routeHandlerBuilder.ProducesProblem(500);

        //return routeHandlerBuilder custom
        return routeHandlerBuilder;
    }

    protected override Delegate Handler => EndpointHandler;

    private static async Task<IResult> EndpointHandler([AsParameters] FilterTodoRequest request, IDbContext context, CancellationToken ct)
    {
        var query = context.Set<Todo>().AsQueryable();
        if(request.IsCompleted.HasValue)
            query = query.Where(x => x.IsCompleted == request.IsCompleted);
        return Results.Ok(await query.ToListAsync(ct));
    }
}
```

### Available Endpoint Base Classes

QuickApi provides several base classes to simplify the creation of endpoints (each adds default `Produces(...)` metadata for common status codes):

- **`PostMinimalEndpoint`**: For POST requests.
- **`GetMinimalEndpoint`**: For GET requests.
- **`PutMinimalEndpoint`**: For PUT requests.
- **`DeleteMinimalEndpoint`**: For DELETE requests.
- **`PatchMinimalEndpoint`**: For PATCH requests.
- **`FilterMinimalEndpoint`**: For filtered GET requests returning `List<T>`.
- **`FilterPaginateMinimalEndpoint`**: For paginated and filtered GET requests returning `PaginatedResult<T>`.
- **`GetFileMinimalEndpoint`**: For serving files using `FileResult`.

#### Paginated filtering example

```csharp
using MediatR;
using QuickApi.Engine.Web.Endpoints.Impl;
using QuickApi.Engine.Web.Models;

public record FilterTodosPaginatedRequest(int PageIndex = 1, int PageSize = 20)
    : IRequest<PaginatedResult<Todo>>;

public class FilterTodosPaginatedEndpoint()
    : FilterPaginateMinimalEndpoint<FilterTodosPaginatedRequest, Todo>("todos/paginated");
```

`PaginatedResult<T>` exposes `Items`, `TotalCount`, `PageIndex`, and `PageSize`.

#### File endpoint example

```csharp
using MediatR;
using QuickApi.Engine.Web.Endpoints.Impl;
using QuickApi.Engine.Web.Models;

public record DownloadInvoiceRequest(Guid Id) : IRequest<FileResult>;

public class DownloadInvoiceEndpoint()
    : GetFileMinimalEndpoint<DownloadInvoiceRequest, FileResult>("invoices/{id:guid}");
```

### Commands/Queries & Model Binding

- Your request types (commands/queries) are plain classes/records that implement MediatR’s `IRequest`/`IRequest<T>` (or your CQRS equivalent). They are passed directly to the `IMessage` pipeline, so handlers stay unchanged.
- You can compose requests with standard ASP.NET binding attributes:

```csharp
using MediatR;
using Microsoft.AspNetCore.Mvc;

// Command mixing route + body binding
public record UpdateTodoRequest(
    [FromRoute] Guid Id,
    [FromBody] UpdateTodoBody Body) : IRequest;

public record UpdateTodoBody(string Title, string? Description);

public class UpdateTodoEndpoint()
    : PutMinimalEndpoint<UpdateTodoRequest>("todos/{id:guid}");
```

```csharp
using MediatR;
using Microsoft.AspNetCore.Mvc;

// Query with query-string binding
public record FilterTodosRequest(
    [FromQuery] bool? IsCompleted,
    [FromQuery] int PageIndex = 1,
    [FromQuery] int PageSize = 20) : IRequest<PaginatedResult<Todo>>;

public class FilterTodosEndpoint()
    : FilterPaginateMinimalEndpoint<FilterTodosRequest, Todo>("todos");
```

### Response conventions

- `GetMinimalEndpoint` returns `200` + `404` metadata.
- `PostMinimalEndpoint` returns `201` + validation problem metadata.
- `PutMinimalEndpoint`, `PatchMinimalEndpoint`, and `DeleteMinimalEndpoint` return `204` + `404` + validation problem metadata.
- `FilterMinimalEndpoint` returns `200 List<T>`; `FilterPaginateMinimalEndpoint` returns `200 PaginatedResult<T>`.

### Example Project

A fully working example project is available. You can clone it and start it with the following command:

```bash
docker compose up --build
```

The application will be accessible at [https://api.localhost/scalar/v1](https://api.localhost/scalar/v1), where you can test more advanced features.

### Note

When inheriting from a class like `PostMinimalEndpoint`, you can add security policies to your endpoint by passing them as a `params` array in the constructor after the route.

Packages will follow to simplify this integration for tools like MediatR and Wolverine.

## Contributing

Contributions are welcome! If you find an issue or have an idea for improvement, feel free to submit a pull request or open an issue on the [GitHub repository](https://github.com/Walfhand/QuickApi).
