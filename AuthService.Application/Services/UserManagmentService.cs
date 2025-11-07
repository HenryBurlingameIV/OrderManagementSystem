using AuthService.Application.Contracts;
using AuthService.Application.DTO;
using AuthService.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.Services
{
    internal class UserManagmentService : IUserManagmentService
    {
        private readonly IEFRepository<User, Guid> _usersRepository;
        private readonly IRoleProvider _roleProvider;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IValidator<CreateUserRequest> _createUserValidator;
        private readonly ILogger<UserManagmentService> _logger;

        public UserManagmentService(
            IEFRepository<User, Guid> usersRepository,
            IRoleProvider roleProvider,
            IPasswordHasher passwordHasher,
            IValidator<CreateUserRequest> createUserValidator,
            ILogger<UserManagmentService> logger)
        {
            _usersRepository = usersRepository;
            _roleProvider = roleProvider;
            _passwordHasher = passwordHasher;
            _createUserValidator = createUserValidator;
            _logger = logger;
        }
        public async Task ActivateUserAsync(Guid userId, CancellationToken ct)
        {
            var user = await _usersRepository.GetByIdAsync(userId, ct);
            if (user == null)
            {
                throw new NotFoundException($"User with ID {userId} not found.");
            }

            if(user.IsActive is true)
            {
                _logger.LogInformation("User with ID {@UserId} is already active.", userId);
                return;
            }

            user.IsActive = true;
            await _usersRepository.SaveChangesAsync(ct);
            _logger.LogInformation("User with ID {@UserId} activated.", userId);
        }

        public Task AssignRoleAsync(Guid userId, List<string> roles, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task<Guid> CreateUserAsync(CreateUserRequest request, CancellationToken ct)
        {
            await _createUserValidator.ValidateAndThrowAsync(request, ct);
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var roles = await _roleProvider.GetRolesByNamesAsync(request.Roles, ct);
            var foundRoleNames = roles.Select(r => r.Name).ToList();
            var missingRoles = request.Roles.Except(foundRoleNames).ToList();

            if (missingRoles.Any())
            {
                throw new ValidationException($"The following roles were not found: {string.Join(", ", missingRoles)}");
            }

            var user = new User()
            {
                Name = request.Name.Trim(),
                Email = request.Email,
                IsActive = request.IsActive,
                HashedPassword = _passwordHasher.HashPassword(request.Password),
                Roles = roles.ToList(),
            };

            await _usersRepository.InsertAsync(user, ct);
            await _usersRepository.SaveChangesAsync(ct);
            return user.Id;               
        }

        public async Task DeactivateUserAsync(Guid userId, CancellationToken ct)
        {
            var user = await _usersRepository.GetByIdAsync(userId, ct);
            if (user == null)
            {
                throw new NotFoundException($"User with ID {userId} not found.");
            }

            if (user.IsActive is false)
            {
                _logger.LogInformation("User with ID {@UserId} is already deactivated.", userId);
                return;
            }

            user.IsActive = false;
            await _usersRepository.SaveChangesAsync(ct);
            _logger.LogInformation("User with ID {@UserId} deactivated.", userId);
        }

        public Task<UserViewModel> GetUserAsync(Guid userId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task RemoveRoleAsync(Guid userId, string role, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
