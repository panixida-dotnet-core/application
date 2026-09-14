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
- FluentValidation extensions for converting single-property and complex domain factory `Result<T>` errors into validation failures.
- Event bus and event handler abstractions for `DomainEvent` integration.
- Unit of work, read repository, and aggregate tracker abstractions for application persistence boundaries.
- `IReadModel` marker interface for immutable read-side result models.
- Read-side helper models for page-based pagination, cursor pagination, multi-field sorting, filtering, and validated result limits.
- Immutable sorting criteria, optional default merging, and generated read-model-specific FluentValidation validators.

## Requirements

- .NET 10 SDK
- Nullable reference types enabled in consuming projects is recommended

## Installation

```xml
<ItemGroup>
  <PackageReference Include="PANiXiDA.Core.Application" Version="4.0.0" />
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

public sealed record NameReadModel(Guid Id, string Name) : IReadModel;

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

Query result payloads should implement `IReadModel`. Collections, pagination
models, and result wrappers may contain read models, but domain entities,
aggregate roots, value objects, enumerations, and strongly typed identifiers
must not cross the read-side boundary.

Concrete read models and custom filters should be declared as records.
Because marker interfaces cannot enforce the declaration kind, consuming
applications should protect this convention with architecture tests.

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

### Limited Queries

`LimitParameters` carries the requested result count without pagination and defaults to 20. Its validator
accepts values from 1 through 200. Constructing the parameters preserves the supplied
value; validation reports invalid input without clamping it.

```csharp
using FluentValidation;
using PANiXiDA.Core.Application.Querying.Limiting;

public sealed record GetOptionsQuery(LimitParameters Limit);

public sealed class GetOptionsQueryValidator : AbstractValidator<GetOptionsQuery>
{
    public GetOptionsQueryValidator()
    {
        RuleFor(query => query.Limit)
            .NotNull()
            .SetValidator(new LimitParametersValidator());
    }
}
```

For example, `new GetOptionsQuery(new LimitParameters())` uses the default limit of 20
and passes validation. An explicit limit overrides the default.
Use `NotNull()` alongside `SetValidator()` to reject missing parameters.

### Pagination Validation

`PaginationParametersValidator` requires a positive `PageNumber`, a `PageSize` from
1 through 200, and an offset that fits in `Int32`. Compose it into the query validator:

```csharp
using FluentValidation;
using PANiXiDA.Core.Application.Querying.Pagination;

public sealed record GetPageQuery(PaginationParameters Pagination);

public sealed class GetPageQueryValidator : AbstractValidator<GetPageQuery>
{
    public GetPageQueryValidator()
    {
        RuleFor(query => query.Pagination)
            .NotNull()
            .SetValidator(new PaginationParametersValidator());
    }
}
```

Validation leaves the supplied parameters unchanged. Existing `Skip` and `Take`
calculations keep their behavior; validate the parameters before using them in a query.

### Sorting

`SortingParameters` is a positional record containing a `SortField[]` in its order
of precedence. Each criterion has a public read model `Field` path and a `SortDirection`
with short members `Asc` and `Desc`.
Neither empty sorting nor default merging adds an implicit `Id` criterion.

```csharp
using PANiXiDA.Core.Application.Querying.Sorting;

var sorting = SortingParameters.Of(
    new SortField("department.name", SortDirection.Desc),
    new SortField("name"));

var defaults = SortingParameters.Descending("createdAt");
var effectiveSorting = sorting.WithDefault(defaults);
```

This produces `department.name` descending, `name` ascending, then `createdAt`
descending. Explicit fields keep their position and direction; defaults with the
same field path are skipped using ordinal case-insensitive comparison. Missing
default fields are appended in their original order. Both inputs stay unchanged.
Validate both inputs before merging. Invalid criteria are not silently repaired
by validation.

`SortingParameters.None`, `SortingParameters.Default()`, and `SortingParameters.Of()` all
represent empty sorting. Use `Ascending(field)` or `Descending(field)` for one
criterion and `Of(...)` for several criteria. The constructor accepts an array:
`new SortingParameters([new SortField("name")])`. Construction and `Of(...)` retain
the supplied array without copying or validating it. `Fields` is mutable through
its array elements and can be replaced using `with`. Validate before use.

`SortField.TryParse` accepts `field`, `field:asc`, and `field:desc`. Directions are
matched against the enum member names without regard to case; surrounding
whitespace is trimmed. Numeric directions, full words such as `Descending`, empty
path segments, whitespace inside paths, commas, and additional colons are rejected.
Dot-separated paths are literal public field
names, not executable expressions. Parsing checks syntax, not whether the field
exists in a read model.

```csharp
using PANiXiDA.Core.Application.Querying.Sorting;

if (SortField.TryParse("department.name:desc", out var field))
{
    var sorting = new SortingParameters([field]);
}
```

The parser handles individual repeated query values such as
`?sort=department.name:desc&sort=name:asc`. An HTTP adapter binds a `SortField[]`
and constructs `SortingParameters` from it. Query parameter naming, endpoint
metadata, and OpenAPI transformations belong to the HTTP adapter.

### Sorting Validation

`SortingParametersValidator` is an abstract base with a protected constructor
accepting supported read model field paths. It rejects null arrays and null criteria
and validates path syntax, supported fields, directions, and case-insensitive
duplicate paths. Empty sorting is valid, and the number of criteria is unrestricted.

The package includes a source generator. Every concrete source-declared `IReadModel`
automatically receives a `<ReadModelName>SortingValidator` in the model's namespace.
No attributes, manual field lists, or runtime property reflection are needed.
The generated class supplies a static, case-insensitive set of paths to the common
`SortingParametersValidator`; validation rules remain in the library.

Compose the generated validator into the query validator:

```csharp
using FluentValidation;
using PANiXiDA.Core.Application.Querying;
using PANiXiDA.Core.Application.Querying.Sorting;

public sealed record DepartmentReadModel(string Name);
public sealed record UserReadModel(string Name, DepartmentReadModel? Department) : IReadModel;
public sealed record GetUsersQuery(SortingParameters Sorting);

public sealed class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        RuleFor(query => query.Sorting)
            .NotNull()
            .SetValidator(new UserReadModelSortingValidator());
    }
}
```

The generated validator can also be supplied through constructor injection like any
other FluentValidation validator. Register the query validator with the application's
existing validation pipeline; the generator does not register services or infer
which read model a query returns.

Both `"name"` and `nameof(UserReadModel.Name)` are accepted, as are
`"department.name"` and `"Department.Name"`. `nameof(UserReadModel.Department)`
produces only `"Department"`; a nested path can be written as
`$"{nameof(UserReadModel.Department)}.{nameof(DepartmentReadModel.Name)}"`.
Type-qualified strings such as `"UserReadModel.Name"` are not property paths.
JSON attributes and naming policies are ignored. A separate HTTP response model
must follow the same field naming conventions.

The generator includes inherited public instance properties with public getters.
Supported leaves are booleans, characters, numeric primitives, strings, enums,
`Guid`, `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, `TimeSpan`, and their
nullable forms. Nested DTO properties are traversed, including properties whose
types come from referenced assemblies. Arrays, collections, indexers, static or
unreadable properties, `object`, `dynamic`, and other `System` types are excluded.
Traversal stops before revisiting the same type definition in a path, so recursive
branches such as `Parent.Parent.Name` are not available. No implicit `Id` is added.

Abstract models do not receive validators; their properties are included in
concrete derived models. Generic read models or models inside generic types report
`PANSG001`; use a concrete model instead. Nested model names include their containing
types, for example `Outer_UserReadModelSortingValidator`. Generated classes are
public only when the model and all its containing types are public; otherwise they
are internal. Validator name collisions report `PANSG002`, and supported paths
that differ only by case report `PANSG003`.

Errors preserve indexed property paths such as `Sorting.Fields[0].Field` and
`Sorting.Fields[0].Order`. `ValidationBehavior` carries those paths into Result
error metadata.

This package does not apply sorting to `IQueryable`, determine SQL translatability,
or guarantee unique ordering for pagination. Those responsibilities belong to the
persistence adapter and the selected read model. No runtime reflection-based field
discovery or dynamic LINQ dependency is introduced by these sorting types; this is
not a NativeAOT compatibility guarantee for FluentValidation or the full stack.

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

`MustBeValidDomainResult` validates several request values with one domain factory and uses each error's
`Error.FieldMetadataKey` value as the FluentValidation property path. Errors without field metadata fall back
to the current rule path.

```csharp
using FluentValidation;
using PANiXiDA.Core.Application.Validation;
using PANiXiDA.Core.ResultPattern;

public sealed record CreateDamageRangeCommand(
    int MinimumDamage,
    int MaximumDamage);

public sealed class CreateDamageRangeCommandValidator
    : AbstractValidator<CreateDamageRangeCommand>
{
    public CreateDamageRangeCommandValidator()
    {
        RuleFor(command => command)
            .MustBeValidDomainResult(command => DamageRange.Create(
                command.MinimumDamage,
                command.MaximumDamage));
    }
}

public sealed record DamageRange(
    int MinimumDamage,
    int MaximumDamage)
{
    public static Result<DamageRange> Create(
        int minimumDamage,
        int maximumDamage)
    {
        if (maximumDamage < minimumDamage)
        {
            return Result.Failure<DamageRange>(
                Error.Validation(
                        "Maximum damage cannot be less than minimum damage.")
                    .WithField(nameof(MaximumDamage)));
        }

        return Result.Success(
            new DamageRange(minimumDamage, maximumDamage));
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
`IReadModel` payloads, optionally wrapped in collections or pagination models.
Read repository contracts must not use types from the Domain layer.
The aggregate repository contract is intentionally not defined by this package; reference `PANiXiDA.Core.Domain` when a repository works with aggregate roots.

## API Overview

### Messaging

- `IMediator` dispatches commands and queries.
- `ICommandHandler<TCommand, TResult>` handles state-changing requests.
- `IQueryHandler<TQuery, TResult>` handles read-only requests.
- `IReadModel` identifies query and read repository result payloads.
- `IBeforeRequestBehavior<TRequest, TResult>` runs before a handler and returns `Result.Success()` to continue request processing, or a failed `Result` to stop it.
- `IAfterRequestBehavior<TRequest, TResult>` runs after a handler returns a result and is defined in the mediator behavior abstractions namespace.
- `IFinallyRequestBehavior<TRequest, TResult>` runs after request processing completes or fails and is defined in the mediator behavior abstractions namespace.

### Validation

- `MustBeValidDomainValue` validates a property through a domain factory that returns `Result<T>` and maps failed result errors to FluentValidation failures.
- `MustBeValidDomainResult` validates a property or request through a domain factory and maps error field metadata to FluentValidation property paths.

### Domain Events

- `IEventBus` publishes domain events.
- `IEventHandler<TEvent>` handles a specific domain event type.
- `IAggregateTracker` tracks aggregate roots touched during a request so their domain events can be published and cleared.

### Persistence

- `IUnitOfWork` defines persistence and transaction operations.
- `IReadRepository<TId>` defines read-only `ExistsByIdAsync` and `AnyAsync` checks.
- Aggregate persistence uses `IRepository<TId, TAggregateRoot>` from `PANiXiDA.Core.Domain`.

### Querying Models

- `IReadModel` identifies immutable read-side result models.
- `PaginationParameters` calculates `Skip` and `Take` for page-based reads.
- `PaginationResult<TItem>` returns page metadata and items.
- `CursorPaginationParameters` represents cursor pagination input.
- `CursorPaginationResult<TItem>` returns cursor pagination metadata and items.
- `SortingParameters` is a positional record with a `SortField[]`, a `SortDirection` per field, and optional default merging.
- `SortingParametersValidator` is the abstract base for validating criterion structure, supported fields, and duplicates.
- Generated `<ReadModelName>SortingValidator` classes validate supported CLR paths discovered from `IReadModel` at compile time.
- `IFilter` identifies application query filter records.

## Configuration

The package does not require runtime configuration. Consumers provide concrete implementations for mediator dispatch, persistence, event bus delivery, aggregate tracking, and dependency injection registration.

## Development

### Restore

```bash
dotnet restore
```

### Format

Build once before formatting a fresh checkout so the formatter can load the
source generator and resolve generated validators.

```bash
dotnet build --no-restore
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

### Continuous integration

Every pull request and push to `main` runs formatting, tests, and mandatory
SonarQube analysis. Publishing from `main` starts only after the SonarQube
Quality Gate succeeds.

## Project Structure

```text
.
├── src/
│   ├── PANiXiDA.Core.Application/
│   └── PANiXiDA.Core.Application.Generators/
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
