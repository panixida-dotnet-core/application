namespace PANiXiDA.Core.Application.Persistence;

public interface IRepository<TId, TAggregateRoot>
    where TId : struct
    where TAggregateRoot : class, IAggregateRoot
{
    Task<TAggregateRoot?> GetByIdAsync(TId id, CancellationToken cancellationToken);
    void Add(TAggregateRoot aggregateRoot);
    void Update(TAggregateRoot aggregateRoot);
    void Delete(TAggregateRoot aggregateRoot);
}
