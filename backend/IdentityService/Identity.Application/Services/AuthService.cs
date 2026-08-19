using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain;
using Microsoft.Extensions.Logging;
using SharedKernel.Exceptions;

namespace Identity.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasherService passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new ValidationException("Email is already registered.");
        }

        var validRoles = new[] { Role.Student, Role.Instructor, Role.Admin };
        if (!validRoles.Contains(request.Role))
        {
            throw new ValidationException("Invalid role.");
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            Role = request.Role,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await _userRepository.AddAsync(user);

        _logger.LogInformation(
            "User registered: {UserId} ({Email}) with role {Role}",
            user.Id, user.Email, user.Role);

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            User = new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            }
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Login failed: email {Email} not found", request.Email);
            throw new ValidationException("Invalid email or password.");
        }

        var isValidPassword = _passwordHasher.VerifyPassword(user, user.PasswordHash, request.Password);
        if (!isValidPassword)
        {
            _logger.LogWarning("Login failed: invalid password for {Email}", request.Email);
            throw new ValidationException("Invalid email or password.");
        }

        _logger.LogInformation("User logged in: {UserId} ({Email})", user.Id, user.Email);

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            User = new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            }
        };
    }
}
