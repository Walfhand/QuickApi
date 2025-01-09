# QuickApi.Engine

[![NuGet](https://img.shields.io/nuget/v/Walfhand.QuickApi.svg)](https://www.nuget.org/packages/Walfhand.QuickApi)
[![Build Status](https://github.com/Walfhand/QuickApi/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/Walfhand/QuickApi/actions)

## Overview

QuickApi is a lightweight library designed to simplify the development of Minimal APIs in .NET applications. It provides seamless integration with popular CQRS tools like MediatR and Wolverine, enabling developers to quickly set up and scale their APIs with minimal boilerplate code.

## Features

- **Effortless Minimal API Setup**: Simplifies configuration and reduces the boilerplate code.
- **CQRS Integration**: Built-in support for MediatR and Wolverine for handling commands, queries, and events.
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

### Step 5: Adding Endpoints

With QuickApi, you can define endpoints anywhere in your code by implementing one of the provided endpoint base classes.

#### Example: Adding a Todo Item without Policies

The following example uses MediatR's CQRS implementation (`IRequest` and `IRequestHandler`). The specific interfaces will vary depending on your chosen CQRS framework:

```csharp
using MediatR;
using QuickApi.Engine.Web.Endpoints.Impl;
using QuickApi.Example.Features.Todos.Domain;

namespace QuickApi.Example.Features.Todos.AddTodo.Endpoints;

// MediatR specific: IRequest<Todo>
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

// MediatR specific: IRequest<Todo>
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

QuickApi provides several base classes to simplify the creation of endpoints:

- **`PostMinimalEndpoint`**: For POST requests.
- **`GetMinimalEndpoint`**: For GET requests.
- **`PutMinimalEndpoint`**: For PUT requests.
- **`DeleteMinimalEndpoint`**: For DELETE requests.
- **`PatchMinimalEndpoint`**: For PATCH requests.
- **`FilterMinimalEndpoint`**: For filtered GET requests.
- **`FilterPaginateMinimalEndpoint`**: For paginated and filtered GET requests.
- **`GetFileMinimalEndpoint`**: For serving files.

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
