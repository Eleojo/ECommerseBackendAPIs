using Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.AuthenticationServices
{
    public interface IAuthenticationService
    {
        string GenerateJwtToken(User user);
        bool VerifyPassword(User user, string password);
        string HashPassword(string password);
    }
}
