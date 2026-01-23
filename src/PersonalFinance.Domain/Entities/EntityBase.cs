namespace PersonalFinance.Domain.Entities;

public abstract class EntityBase
{
    public Guid Id { get; }

    protected EntityBase()
    {
        Id = Guid.NewGuid();
    }
}
