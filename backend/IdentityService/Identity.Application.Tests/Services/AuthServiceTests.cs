using FluentAssertions;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Application.Services;
using Identity.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using SharedKernel.Exceptions;

namespace Identity.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasherService> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasherService>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        _authService = new AuthService(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowValidationException_WhenEmailAlreadyExists()
    {
        // Arrange
        var request = new RegisterRequest { Email = "test@test.com", Name = "Test", Password = "password", Role = Role.Student };
        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync(new User());

        // Act
        Func<Task> act = async () => await _authService.RegisterAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Email is already registered.");
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowValidationException_WhenRoleIsInvalid()
    {
        // Arrange
        var request = new RegisterRequest { Email = "new@test.com", Name = "Test", Password = "password", Role = "InvalidRole" };
        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _authService.RegisterAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Invalid role.");
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnToken_WhenRegistrationIsSuccessful()
    {
        // Arrange
        var request = new RegisterRequest { Email = "new@test.com", Name = "Test", Password = "password", Role = Role.Student };
        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);
        _passwordHasherMock.Setup(hasher => hasher.HashPassword(It.IsAny<User>(), request.Password))
            .Returns("hashed_password");
        _jwtTokenGeneratorMock.Setup(generator => generator.GenerateToken(It.IsAny<User>()))
            .Returns("valid_token");

        // Act
        var response = await _authService.RegisterAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Token.Should().Be("valid_token");
        response.User.Email.Should().Be(request.Email);
        _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowValidationException_WhenEmailNotFound()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@test.com", Password = "password" };
        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _authService.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowValidationException_WhenPasswordIsInvalid()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@test.com", Password = "wrong_password" };
        var user = new User { Email = request.Email, PasswordHash = "hashed_password" };
        
        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(hasher => hasher.VerifyPassword(user, user.PasswordHash, request.Password))
            .Returns(false);

        // Act
        Func<Task> act = async () => await _authService.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenLoginIsSuccessful()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@test.com", Password = "password" };
        var user = new User { Email = request.Email, PasswordHash = "hashed_password" };
        
        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(hasher => hasher.VerifyPassword(user, user.PasswordHash, request.Password))
            .Returns(true);
        _jwtTokenGeneratorMock.Setup(generator => generator.GenerateToken(user))
            .Returns("valid_token");

        // Act
        var response = await _authService.LoginAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Token.Should().Be("valid_token");
        response.User.Email.Should().Be(request.Email);
    }
}
