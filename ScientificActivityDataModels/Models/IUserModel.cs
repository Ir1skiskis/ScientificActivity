using ScientificActivityDataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityDataModels.Models
{
    public interface IUserModel : IId
    {
        string Email { get; }
        string PasswordHash { get; }
        UserRole Role { get; }
        bool IsActive { get; }
    }
}
