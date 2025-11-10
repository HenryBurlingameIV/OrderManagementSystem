using AuthService.Application.Contracts;
using AuthService.Application.DTO;
using AuthService.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
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

        public async Task AssignRoleAsync(Guid userId, string roleName, CancellationToken ct)
        {
            var user = await _usersRepository.GetFirstOrDefaultAsync(
                filter: u => u.Id == userId,
                include: q => q.Include(u => u.Roles),
                asNoTraсking: false,
                ct: ct);

            if (user == null)
            {
                throw new NotFoundException($"User with ID {userId} not found.");
            }

            var role = await _roleProvider.GetRoleByNameAsync(roleName, ct);
            if (role == null)
            {
                throw new NotFoundException($"Role {roleName} not found.");
            }

            if(user.Roles.Any(r => r.Id == role.Id))
            {
                _logger.LogWarning("Role {RoleName} is already assigned to user {UserId}", roleName, userId);
                return;
            }

            user.Roles.Add(role);
            await _usersRepository.SaveChangesAsync(ct);
            _logger.LogInformation("New role {@RoleName} assigned to user {@UserId}", roleName, userId);
        }

        public async Task<Guid> CreateUserAsync(CreateUserRequest request, CancellationToken ct)
        {
            await _createUserValidator.ValidateAndThrowAsync(request, ct);

            var roles = await _roleProvider.GetRolesByNamesAsync(request.Roles, ct);

            if (roles.Count != request.Roles.Count)
            {
                var foundRoleNames = roles.Select(r => r.Name).ToList();
                var missingRoles = request.Roles.Except(foundRoleNames, StringComparer.OrdinalIgnoreCase).ToList();

                throw new ValidationException($"The following roles were not found: {string.Join(", ", missingRoles)}");
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var user = new User()
            {
                Name = request.Name.Trim(),
                Email = normalizedEmail,
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

        public async Task RemoveRoleAsync(Guid userId, string roleName, CancellationToken ct)
        {
            var role = await _roleProvider.GetRoleByNameAsync(roleName, ct);
            if(role is null)
            {
                throw new NotFoundException($"Role {roleName} not found.");
            }

            var user = await _usersRepository.GetFirstOrDefaultAsync(
                filter: u => u.Id == userId,
                include: q => q.Include(u => u.Roles),
                asNoTraсking: false,
                ct: ct);

            if (user == null)
            {
                throw new NotFoundException($"User with ID {userId} not found.");
            }

            var userRole = user.Roles.FirstOrDefault(r => r.Id == role.Id);
;
            if (userRole == null)
            {
                _logger.LogWarning("User {@UserId} doesn't have role {@RoleName}.", userId, roleName);
                return;
            }
            user.Roles.Remove(userRole);
            await _usersRepository.SaveChangesAsync(ct);
            _logger.LogInformation("Role {@RoleName} removed from user {@UserId}", roleName, userId);
        }
    }
}
