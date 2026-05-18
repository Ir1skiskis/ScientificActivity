using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityBusinessLogics.Services
{
    public class PasswordHashService
    {
        private readonly PasswordHasher<object> _passwordHasher = new();

        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Пароль не может быть пустым", nameof(password));
            }

            return _passwordHasher.HashPassword(new object(), password);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
            {
                return false;
            }

            try
            {
                var result = _passwordHasher.VerifyHashedPassword(
                    new object(),
                    passwordHash,
                    password);

                return result == PasswordVerificationResult.Success ||
                       result == PasswordVerificationResult.SuccessRehashNeeded;
            }
            catch (FormatException)
            {
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
