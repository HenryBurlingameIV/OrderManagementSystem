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
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            IEFRepository<User, Guid> usersRepository,
            IRoleProvider roleProvider,
            IJwtProvider jwtProvider,
            IPasswordHasher passwordHasher,
            ILogger<AuthenticationService> logger)
        {
            _usersRepository = usersRepository;
            _roleProvider = roleProvider;
            _jwtProvider = jwtProvider;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct)
        {
            var user = await _usersRepository.GetFirstOrDefaultAsync(
                filter: x => x.Email == request.Email,
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
            return new LoginResponse(token);
        }

        public async Task RegisterAsync(RegisterRequest request, CancellationToken ct)
        {
            var emailIsExists = await _usersRepository.ExistsAsync(
                predicate: u => u.Email == request.Email,
                ct: ct);

            if (emailIsExists)
            {
                throw new ValidationException("Such email already used.");
            }

            var user = new User()
            {
                Name = request.Name,
                Email = request.Email,
                IsActive = true,
                HashedPassword = _passwordHasher.HashPassword(request.Password)
            };

            user.Roles.Add(await _roleProvider.GetClientRoleAsync(ct));
            await _usersRepository.InsertAsync(user, ct);
            await _usersRepository.SaveChangesAsync(ct);
        }

    }
}
