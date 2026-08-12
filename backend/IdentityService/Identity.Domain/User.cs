using SharedKernel;

namespace Identity.Domain;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
}
