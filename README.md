# PANiXiDA.Core.Application

Application-layer contracts and helpers for .NET 10 services using CQRS and DDD.
Concrete mediator, persistence, transport, and DI implementations belong to adapters.

## Status

[![CI](https://github.com/panixida-dotnet-core/application/actions/workflows/ci.yml/badge.svg)](https://github.com/panixida-dotnet-core/application/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/PANiXiDA.Core.Application.svg)](https://www.nuget.org/packages/PANiXiDA.Core.Application)
[![NuGet downloads](https://img.shields.io/nuget/dt/PANiXiDA.Core.Application.svg)](https://www.nuget.org/packages/PANiXiDA.Core.Application)
[![Target Framework](https://img.shields.io/badge/target-net10.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg)](LICENSE)

## Features

- CQRS contracts, mediator interfaces, and request behaviors.
- FluentValidation integration with Result errors.
- Domain event publishing, unit of work, and repository abstractions.
- Read models, pagination, limits, and multi-field sorting with generated validators.

## Requirements

- .NET 10 SDK
- Nullable reference types recommended

## Installation

```xml
<ItemGroup>
  <PackageReference Include="PANiXiDA.Core.Application" Version="4.0.0" />
</ItemGroup>
```

## Basic Usage

### Commands and Queries

```csharp
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Messaging.Mediator.Handlers;
using PANiXiDA.Core.Application.Querying;
using PANiXiDA.Core.ResultPattern;

public sealed record PingCommand : ICommand<Result>;
public sealed record NameReadModel(Guid Id, string Name) : IReadModel;
public sealed record GetNameQuery(Guid Id) : IQuery<Result<NameReadModel>>;

public sealed class GetNameQueryHandler : IQueryHandler<GetNameQuery, Result<NameReadModel>>
{
    public Task<Result<NameReadModel>> HandleAsync(GetNameQuery query, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(new NameReadModel(query.Id, "PANiXiDA")));
    }
}
```

Use records for read models and custom `IFilter` implementations. Read contracts
should expose primitive values and `IReadModel` payloads rather than domain objects.

### Pagination and Limits

```csharp
using PANiXiDA.Core.Application.Querying.Cursor;
using PANiXiDA.Core.Application.Querying.Limiting;
using PANiXiDA.Core.Application.Querying.Pagination;

var page = PaginationResult<string>.Create(
    items: ["first", "second"], pageNumber: 1, pageSize: 10, totalCount: 2);

var cursorPage = CursorPaginationResult<string>.Create(
    items: ["first", "second"], limit: 10, nextCursor: "cursor-2", hasNextPage: true);

var limit = new LimitParameters();
```

`LimitParameters` defaults to 20; `LimitParametersValidator` accepts 1–200.
`PaginationParametersValidator` requires a positive page number, page size 1–200,
and an offset fitting in `Int32`. Validate before querying; values are not clamped.

Compose parameter validators into query validators with `NotNull().SetValidator(...)`:

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

### Sorting

`SortingParameters(SortField[] Fields)` preserves criterion order. Directions are
`SortDirection.Asc` and `Desc`; empty sorting adds no implicit `Id`.

```csharp
using PANiXiDA.Core.Application.Querying.Sorting;

var sorting = SortingParameters.Of(
    new SortField("department.name", SortDirection.Desc),
    new SortField("name"));

var effectiveSorting = sorting.WithDefault(SortingParameters.Descending("createdAt"));

SortField.TryParse("department.name:desc", out var field);
```

`WithDefault` appends missing fields, preserving explicit directions and precedence.
Paths are compared without regard to case. Use `None` for empty sorting; `Ascending`
and `Descending` create a single criterion. The supplied array is retained; validate
parameters before use or merging.

`TryParse` accepts `field`, `field:asc`, and `field:desc` without regard to case.
It parses individual values such as those in `?sort=name:asc&sort=department.name:desc`.
HTTP binding and OpenAPI documentation belong to the transport adapter.

### Sorting Validation

The included generator creates `<ReadModelName>SortingValidator` for concrete
source-declared `IReadModel` types, without attributes or runtime property reflection.

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

The abstract `SortingParametersValidator` holds all rules: non-null criteria, valid
paths and directions, supported fields, and no duplicates. Empty sorting is valid;
there is no criterion count limit. Errors retain paths such as `Sorting.Fields[0].Field`.

- Paths use public scalar CLR properties, including inherited and nested properties.
  `name` and `nameof(UserReadModel.Name)` are equivalent; JSON renames are ignored.
- Collections, indexers, unreadable properties, and recursive branches are excluded.
  Generic roots and ambiguous names produce compiler errors.
- Register the query validator in the application's validation pipeline.
  Applying sorting to `IQueryable` belongs to the persistence adapter.

## Request Behaviors

Register behaviors in pipeline order:

```text
before:  ValidationBehavior
before:  BeginTransactionBehavior
handler: ICommandHandler<TCommand, TResult>
after:   PublishDomainEventsBehavior
after:   CommitTransactionBehavior
finally: CleanupTransactionBehavior
```

`ValidationBehavior` runs registered FluentValidation validators and preserves error
field metadata in `Result`. A failed before-stage result stops handler execution.
The other behaviors manage transactions, domain events, and cleanup.

## Domain Value Validation

`MustBeValidDomainValue` maps domain factory failures to the current property:

```csharp
using FluentValidation;
using PANiXiDA.Core.Application.Validation;
using PANiXiDA.Core.ResultPattern;

public sealed record CreateUserCommand(string Email);

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.Email).MustBeValidDomainValue(Email.Create);
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

For complex validation, `MustBeValidDomainResult` preserves `Error.FieldMetadataKey`
paths; errors without a field are attached to the current rule path.

## Repository Abstraction Ownership

| Contract | Package | Purpose |
| --- | --- | --- |
| `IReadRepository<TId>` | Application | Read-side `ExistsByIdAsync` and `AnyAsync`. |
| `IRepository<TId, TAggregateRoot>` | [Domain](https://github.com/panixida-dotnet-core/domain#repository-abstraction-ownership) | Aggregate persistence. |

Read repository identifiers should be primitive values such as `Guid`.
Additional read methods should accept primitive/application parameters and return
`IReadModel` payloads, optionally wrapped in collections or pagination models.

## API Overview

| Area | Main contracts |
| --- | --- |
| Messaging | `ICommand`, `IQuery`, `IRequest`, `IMediator`, command/query handlers |
| Behaviors | `IBeforeRequestBehavior`, `IAfterRequestBehavior`, `IFinallyRequestBehavior` |
| Domain events | `IEventBus`, `IEventHandler`, `IAggregateTracker` |
| Persistence | `IUnitOfWork`, `IReadRepository` |
| Querying | `IReadModel`, `IFilter`, pagination, cursors, limits, sorting |

## Configuration

No runtime configuration is required. Register concrete mediator, persistence,
event bus, and validation implementations in the consuming application.

## Development

Build before formatting a fresh checkout so the formatter can load the generator.

```bash
dotnet restore
dotnet build --no-restore
dotnet format
dotnet build --configuration Release
dotnet test --configuration Release
dotnet pack --configuration Release
```

CI runs formatting, tests, and mandatory SonarQube analysis. Publishing from `main`
requires a successful Quality Gate.

## Project Structure

```text
src/
├── PANiXiDA.Core.Application/
└── PANiXiDA.Core.Application.Generators/
tests/
└── PANiXiDA.Core.Application.UnitTests/
```

## License

[Apache-2.0](LICENSE).
