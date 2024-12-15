# QuickApi

![NuGet Version](https://img.shields.io/nuget/v/Walfhand.QuickApi)
![NuGet Downloads](https://img.shields.io/nuget/dt/Walfhand.QuickApi)

## Overview

QuickApi is a lightweight library designed to simplify the development of Minimal APIs in .NET applications. It provides seamless integration with popular CQRS tools like MediatR and Wolverine, enabling developers to quickly set up and scale their APIs with minimal boilerplate code.

## Features

- **Effortless Minimal API Setup**: Simplifies configuration and reduces the boilerplate code.
- **CQRS Integration**: Built-in support for MediatR and Wolverine for handling commands, queries, and events.
- **Developer Friendly**: Focuses on improving productivity and code readability.
- **NuGet Package**: Easily installable via NuGet.

## Installation

QuickApi is available on [NuGet](https://www.nuget.org/packages/Walfhand.QuickApi/):

```bash
Install-Package Walfhand.QuickApi
```

Or, using the .NET CLI:

```bash
dotnet add package Walfhand.QuickApi
```

## Getting Started

### Step 1: Installation

Install the package using the .NET CLI:

```bash
dotnet add package Walfhand.QuickApi
```

### Step 2: Register QuickApi in your Program.cs

Add the following lines to your `Program.cs` file:

```csharp
builder.Services.AddMinimalEndpoints();
app.UseMinimalEndpoints();
```

### Step 3: Implement the IMessage Interface

Implement the `IMessage` interface to use your preferred CQRS tool. Here is an example using MediatR:

```csharp
using MediatR;
using QuickApi.Engine.Web.Cqrs;

namespace QuickApi.Example.Cqrs;

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

Register your `MessageService` implementation in the `Program.cs` file:

```csharp
builder.Services.AddScoped<IMessage, MessageService>();
```

### Step 5: Adding Endpoints

With QuickApi, you can define endpoints anywhere in your code by implementing one of the provided endpoint base classes.

#### Example: Adding a Todo Item without Policies

```csharp
using MediatR;
using QuickApi.Engine.Web.Endpoints.Impl;
using QuickApi.Example.Features.Todos.Domain;

namespace QuickApi.Example.Features.Todos.AddTodo.Endpoints;

public record AddTodoRequest(string Title, string? Description) : IRequest<Todo>;

public class AddTodoEndpoint() : PostMinimalEndpoint<AddTodoRequest, Todo>("todos")
{
}

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

```csharp
using MediatR;
using QuickApi.Engine.Web.Endpoints.Impl;
using QuickApi.Example.Features.Todos.Domain;

namespace QuickApi.Example.Features.Todos.AddTodo.Endpoints;

public record AddTodoRequest(string Title, string? Description) : IRequest<Todo>;

public class AddTodoEndpoint() : PostMinimalEndpoint<AddTodoRequest, Todo>(
    "todos", 
    nameof(PoliciesEnum.Subscribed))
{
}

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
using Microsoft.AspNetCore.Mvc;
using QuickApi.Engine.Web.Endpoints.Impl;
using QuickApi.Example.Features.Todos.Domain;

namespace QuickApi.Example.Features.Todos.Custom.Endpoints;

public class FilterTodoEndpointCustom() : FilterMinimalEndpoint<FilterTodoRequest, Todo>("todos/custom")
{
    protected override void Configure(RouteHandlerBuilder routeBuilder)
    {
        base.Configure(routeBuilder);
        routeBuilder.ProducesProblem(500);
    }

    protected override Delegate Handler => EndpointHandler;

    private static async Task<IResult> EndpointHandler([AsParameters] FilterTodoRequest request, IDbContext context, CancellationToken ct)
    {
        var query = context.Set<Todo>().AsQueryable();
        if (request.IsCompleted.HasValue)
            query = query.Where(x => x.IsCompleted == request.IsCompleted);
        return Results.Ok(await query.ToListAsync(ct));
    }
}
```

### Available Endpoint Base Classes

QuickApi provides several base classes to simplify the creation of endpoints:

- **`PostMinimalEndpoint<TRequest, TResponse>`**: For POST requests.
- **`GetMinimalEndpoint<TResponse>`**: For GET requests.
- **`PutMinimalEndpoint<TRequest, TResponse>`**: For PUT requests.
- **`DeleteMinimalEndpoint<TRequest>`**: For DELETE requests.
- **`PatchMinimalEndpoint<TRequest, TResponse>`**: For PATCH requests.
- **`FilterMinimalEndpoint<TResponse>`**: For filtered GET requests.
- **`FilterPaginateMinimalEndpoint<TResponse>`**: For paginated and filtered GET requests.
- **`GetFileMinimalEndpoint<TResponse>`**: For serving files.

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


