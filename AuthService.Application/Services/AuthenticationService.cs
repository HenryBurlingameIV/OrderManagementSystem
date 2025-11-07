using AuthService.Application.Contracts;
using AuthService.Application.DTO;
using AuthService.Domain.Entities;
using FluentValidation;

//using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderManagementSystem.Shared.Authorization;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static OrderManagementSystem.Shared.Exceptions.AuthExceptions;

namespace AuthService.Application.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IEFRepository<User, Guid> _usersRepository;
        private readonly IRoleProvider _roleProvider;
        private readonly IJwtProvider _jwtProvider;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IValidator<LoginRequest> _loginValidator;
        private readonly IValidator<RegisterRequest> _registerValidator;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            IEFRepository<User, Guid> usersRepository,
            IRoleProvider roleProvider,
            IJwtProvider jwtProvider,
            IPasswordHasher passwordHasher,
            IValidator<LoginRequest> loginValidator,
            IValidator<RegisterRequest> registerValidator,
            ILogger<AuthenticationService> logger)
        {
            _usersRepository = usersRepository;
            _roleProvider = roleProvider;
            _jwtProvider = jwtProvider;
            _passwordHasher = passwordHasher;
            _loginValidator = loginValidator;
            _registerValidator = registerValidator;
            _logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct)
        {
            await _loginValidator.ValidateAndThrowAsync(request, ct);
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _usersRepository.GetFirstOrDefaultAsync(
                filter: x => x.Email == normalizedEmail,
                include: x => x
                    .Include(u => u.Roles)
                        .ThenInclude(r => r.Permissions),
                ct: ct);

            if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.HashedPassword))
            {
                _logger.LogWarning("Failed login attempt for email: {@Email}", request.Email);
                throw new InvalidCredentialsException();
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Failed login attempt for deactivated user with email: {@Email}", request.Email);
                throw new AccountInactiveException();
            }

            var token = _jwtProvider.GenerateToken(user!);
            _logger.LogInformation("User with ID {UserId} successfully logged on.", user.Id);
            return new LoginResponse(token);
        }

        public async Task RegisterAsync(RegisterRequest request, CancellationToken ct)
        {
            await _registerValidator.ValidateAndThrowAsync(request, ct);
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = new User()
            {
                Name = request.Name.Trim(),
                Email = normalizedEmail,
                IsActive = true,
                HashedPassword = _passwordHasher.HashPassword(request.Password)
            };

            user.Roles.Add(await _roleProvider.GetClientRoleAsync(ct));
            await _usersRepository.InsertAsync(user, ct);
            await _usersRepository.SaveChangesAsync(ct);
            _logger.LogInformation("User with ID {UserId} successfully created.", user.Id);
        }
    }
}
