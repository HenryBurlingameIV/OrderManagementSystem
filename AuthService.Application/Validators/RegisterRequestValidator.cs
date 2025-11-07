using AuthService.Application.DTO;
using AuthService.Domain.Entities;
using FluentValidation;
using OrderManagementSystem.Shared.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AuthService.Application.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        private readonly IEFRepository<User, Guid> _usersRepository;

        public RegisterRequestValidator(IEFRepository<User, Guid> usersRepository)
        {
            _usersRepository = usersRepository;
            RuleFor(r => r.Email)
                .NotEmpty().WithMessage("Email is required")
                .MaximumLength(254).WithMessage("Email is too long.")
                .EmailAddress().WithMessage("Invalid email format")
                .MustAsync(BeUniqueEmail).WithMessage("Such email already used");

            RuleFor(r => r.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password is too short. Minimal length is 8 charachers.")
                .MaximumLength(100).WithMessage("Password is too long. Maximun length is 100 characters");

            RuleFor(r => r.Name)
               .NotEmpty().WithMessage("Name is required.")
               .MinimumLength(2).WithMessage("Name is too short. Minimal length is 2 charachers.")
               .MaximumLength(100).WithMessage("Name is too long. Maximun length is 100 characters");

        }

        private async Task<bool> BeUniqueEmail(string email, CancellationToken ct)
        {
            return !await _usersRepository.ExistsAsync(predicate: u => u.Email == email.Trim().ToLower(), ct: ct);
        }

    }
}
