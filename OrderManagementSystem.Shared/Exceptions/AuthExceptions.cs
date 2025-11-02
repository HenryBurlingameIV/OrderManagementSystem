using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagementSystem.Shared.Exceptions
{
    public class AuthExceptions
    {
        public class AuthenticationException : Exception
        {
            public AuthenticationException(string message) : base(message) { }
        }

        public class InvalidCredentialsException : AuthenticationException
        {
            public InvalidCredentialsException() : base("Invalid email or password") { }
        }

        public class AccountInactiveException : AuthenticationException
        {
            public AccountInactiveException() : base("Account is deactivated") { }
        }

        public class ForbiddenException : AuthenticationException
        {
            public ForbiddenException() : base("Access forbidden") { }

        }
    }
}
