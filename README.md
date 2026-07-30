# PANiXiDA.Core.Application

`PANiXiDA.Core.Application` is a .NET library with application-layer abstractions for Clean Architecture, CQRS, and DDD-based services.

It defines contracts and small reusable building blocks for commands, queries, request behaviors, domain event publishing, unit-of-work orchestration, read repositories, aggregate tracking, and read-side paging helpers. The package intentionally does not provide a concrete mediator, database provider, dependency injection module, or transport-specific implementation.

## Status

[![CI](https://github.com/panixida-dotnet-core/application/actions/workflows/ci.yml/badge.svg)](https://github.com/panixida-dotnet-core/application/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/PANiXiDA.Core.Application.svg)](https://www.nuget.org/packages/PANiXiDA.Core.Application)
[![NuGet downloads](https://img.shields.io/nuget/dt/PANiXiDA.Core.Application.svg)](https://www.nuget.org/packages/PANiXiDA.Core.Application)
[![Target Framework](https://img.shields.io/badge/target-net10.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg)](LICENSE)

## Features

- CQRS request contracts: `ICommand<TResult>`, `IQuery<TResult>`, and `IRequest<TResult>`.
- Mediator contracts for command/query dispatch and handler implementation.
- Pipeline behavior contracts for before, after, and finally request stages, including before-stage success/failure results.
- Built-in behaviors for FluentValidation request validation, transaction start, commit, cleanup, and domain event publishing.
- FluentValidation extension for converting failed domain factory `Result<T>` values into property validation failures.
- Event bus and event handler abstractions for `DomainEvent` integration.
- Unit of work, read repository, and aggregate tracker abstractions for application persistence boundaries.
- `ReadModel` base type for immutable read-side result models.
- Read-side helper models for page-based pagination, cursor pagination, sorting, and filtering.

## Requirements

- .NET 10 SDK
- Nullable reference types enabled in consuming projects is recommended

## Installation

```xml
<ItemGroup>
  <PackageReference Include="PANiXiDA.Core.Application" Version="2.0.0" />
</ItemGroup>
```

## Basic Usage

### Command Contract

```csharp
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Messaging.Mediator.Handlers;
using PANiXiDA.Core.ResultPattern;

public sealed record PingCommand : ICommand<Result>;

public sealed class PingCommandHandler : ICommandHandler<PingCommand, Result>
{
    public Task<Result> HandleAsync(
        PingCommand command,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success());
    }
}
```

### Query Contract

```csharp
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Messaging.Mediator.Handlers;
using PANiXiDA.Core.Application.Querying;
using PANiXiDA.Core.ResultPattern;

public sealed record NameReadModel(Guid Id, string Name) : ReadModel;

public sealed record GetNameQuery(Guid Id) : IQuery<Result<NameReadModel>>;

public sealed class GetNameQueryHandler
    : IQueryHandler<GetNameQuery, Result<NameReadModel>>
{
    public Task<Result<NameReadModel>> HandleAsync(
        GetNameQuery query,
        CancellationToken cancellationToken)
    {
        var readModel = new NameReadModel(query.Id, "PANiXiDA");

        return Task.FromResult(Result.Success(readModel));
    }
}
```

Query result payloads should derive from `ReadModel`. Collections, pagination
models, and result wrappers may contain read models, but domain entities,
aggregate roots, value objects, enumerations, and strongly typed identifiers
must not cross the read-side boundary.

### Page-Based Query Result

```csharp
using PANiXiDA.Core.Application.Querying.Pagination;

var result = PaginationResult<string>.Create(
    items: ["first", "second"],
    pageNumber: 1,
    pageSize: 10,
    totalCount: 2);

var hasNextPage = result.HasNextPage;
```

### Cursor-Based Query Result

```csharp
using PANiXiDA.Core.Application.Querying.Cursor;

var result = CursorPaginationResult<string>.Create(
    items: ["first", "second"],
    limit: 10,
    nextCursor: "cursor-2",
    hasNextPage: true);
```

## Request Behaviors

The package includes reusable mediator behavior implementations for request validation, command transaction orchestration, and domain event publication:

- `ValidationBehavior<TRequest, TResult>` validates requests with registered FluentValidation `IValidator<TRequest>` implementations and returns a failed `Result` before the handler runs when validation fails.
- `BeginTransactionBehavior<TCommand, TResult>` starts a transaction before a command handler runs.
- `PublishDomainEventsBehavior<TRequest, TResult>` publishes domain events collected from tracked aggregate roots after a successful request result and clears tracked events after a failed result or completed successful publication.
- `CommitTransactionBehavior<TCommand, TResult>` commits the active transaction after a successful command result.
- `CleanupTransactionBehavior<TCommand, TResult>` rolls back failed command transactions and disposes transaction resources.

A consuming mediator implementation should register these behaviors in a deterministic order. A typical command pipeline is:

```text
before:  ValidationBehavior
before:  BeginTransactionBehavior
handler: ICommandHandler<TCommand, TResult>
after:   PublishDomainEventsBehavior
after:   CommitTransactionBehavior
finally: CleanupTransactionBehavior
```

The exact registration mechanism depends on the mediator or composition root used by the consuming application.
A consuming mediator should continue to the handler when a before behavior returns `Result.Success()`.
When a before behavior returns a failed `Result`, the mediator should stop the pipeline and return a failed request `TResult` with the same errors.

## Domain Value Validation

`MustBeValidDomainValue` adds a FluentValidation rule that calls a domain value factory returning `Result<T>`.
When the factory fails, each result error message is added as a validation failure for the current property.

```csharp
using FluentValidation;
using PANiXiDA.Core.Application.Validation;
using PANiXiDA.Core.ResultPattern;

public sealed record CreateUserCommand(string Email);

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.Email)
            .MustBeValidDomainValue(Email.Create);
    }
}

public sealed record Email(string Value)
{
    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<Email>(Error.Validation("Email is required."));
        }

        return Result.Success(new Email(value));
    }
}
```

## Repository Abstraction Ownership

Repository contracts are split by architectural responsibility:

| Contract | Package | Namespace | Responsibility |
| --- | --- | --- | --- |
| `IReadRepository<TId>` | `PANiXiDA.Core.Application` | `PANiXiDA.Core.Application.Persistence` | Read-side existence checks used by application queries and validation. |
| `IRepository<TId, TAggregateRoot>` | [`PANiXiDA.Core.Domain`](https://github.com/panixida-dotnet-core/domain#repository-abstraction-ownership) | `PANiXiDA.Core.Domain.Abstractions` | Loading and persisting aggregate roots through the domain boundary. |

`IReadRepository<TId>` provides `ExistsByIdAsync` and `AnyAsync`.
Its identifier should be a primitive read-side value such as `Guid`.
Additional read repository methods may accept primitive values or application
parameter models composed exclusively from primitive values, and should return
`ReadModel` payloads, optionally wrapped in collections or pagination models.
Read repository contracts must not use types from the Domain layer.
The aggregate repository contract is intentionally not defined by this package; reference `PANiXiDA.Core.Domain` when a repository works with aggregate roots.

## API Overview

### Messaging

- `IMediator` dispatches commands and queries.
- `ICommandHandler<TCommand, TResult>` handles state-changing requests.
- `IQueryHandler<TQuery, TResult>` handles read-only requests.
- `ReadModel` is the base type for query and read repository result payloads.
- `IBeforeRequestBehavior<TRequest, TResult>` runs before a handler and returns `Result.Success()` to continue request processing, or a failed `Result` to stop it.
- `IAfterRequestBehavior<TRequest, TResult>` runs after a handler returns a result and is defined in the mediator behavior abstractions namespace.
- `IFinallyRequestBehavior<TRequest, TResult>` runs after request processing completes or fails and is defined in the mediator behavior abstractions namespace.

### Validation

- `MustBeValidDomainValue` validates a property through a domain factory that returns `Result<T>` and maps failed result errors to FluentValidation failures.

### Domain Events

- `IEventBus` publishes domain events.
- `IEventHandler<TEvent>` handles a specific domain event type.
- `IAggregateTracker` tracks aggregate roots touched during a request so their domain events can be published and cleared.

### Persistence

- `IUnitOfWork` defines persistence and transaction operations.
- `IReadRepository<TId>` defines read-only `ExistsByIdAsync` and `AnyAsync` checks.
- Aggregate persistence uses `IRepository<TId, TAggregateRoot>` from `PANiXiDA.Core.Domain`.

### Querying Models

- `ReadModel` identifies immutable read-side result models.
- `PaginationParameters` calculates `Skip` and `Take` for page-based reads.
- `PaginationResult<TItem>` returns page metadata and items.
- `CursorPaginationParameters` represents cursor pagination input.
- `CursorPaginationResult<TItem>` returns cursor pagination metadata and items.
- `SortParameters` and `SortOrder` represent read sorting options.
- `FilterParameters` is the base type for custom read filter records.

## Configuration

The package does not require runtime configuration. Consumers provide concrete implementations for mediator dispatch, persistence, event bus delivery, aggregate tracking, and dependency injection registration.

## Development

### Restore

```bash
dotnet restore
```

### Format

```bash
dotnet format
```

### Build

```bash
dotnet build --configuration Release
```

### Test

```bash
dotnet test --configuration Release
```

### Pack

```bash
dotnet pack --configuration Release
```

## Project Structure

```text
.
├── src/
│   └── PANiXiDA.Core.Application/
├── tests/
│   └── PANiXiDA.Core.Application.UnitTests/
├── Directory.Build.props
├── Directory.Build.targets
├── Directory.Packages.props
├── global.json
├── version.json
├── icon.png
├── LICENSE
└── README.md
```

## License

This project is licensed under the Apache-2.0 license. See the [LICENSE](LICENSE) file for details.
