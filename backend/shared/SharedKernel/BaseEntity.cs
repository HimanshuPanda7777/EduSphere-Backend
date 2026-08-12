namespace SharedKernel;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
