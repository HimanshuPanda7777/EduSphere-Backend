using Identity.Domain;

namespace Identity.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
