using AuthService.Application.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator() 
        {
            RuleFor(r => r.Email)
                .NotEmpty().WithMessage("Email is required")
                .MaximumLength(254)
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(r => r.Password)
                .NotEmpty().WithMessage("Password is required");
        }
    }
}
