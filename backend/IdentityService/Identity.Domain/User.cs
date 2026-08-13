using SharedKernel;

namespace Identity.Domain;

public class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = Identity.Domain.Role.Student;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
