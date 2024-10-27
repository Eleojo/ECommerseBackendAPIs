using Core.AuthenticationServices;
using Data.AppDbContext;
using Data.Dtos;
using Data.Enums;
using Data.Model;
using Microsoft.EntityFrameworkCore;

namespace Core.UserServices
{
    public class UserService : IUserService
    {
        private readonly ECommerceDbContext _context;
        private readonly IAuthenticationService _authenticationService;

        public UserService(ECommerceDbContext context, IAuthenticationService authenticationService)
        {
            _context = context;
            _authenticationService = authenticationService;
        }

        public async Task<bool> Register(UserDto userDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
            {
                return false;
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                PasswordHash = _authenticationService.HashPassword(userDto.Password),
                Role = UserRoleEnum.Admin // Default role
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<string> Login(LoginDto loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null || !_authenticationService.VerifyPassword(user, loginDto.Password))
            {
                return ("Login error check login credentials");
            }

            var token = _authenticationService.GenerateJwtToken(user);

            return token;
        }

        public async Task<bool> SoftDeleteUser(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsDeleted = true;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        public async Task<bool> UpdateUserRole(Guid userId, UserRoleEnum role)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.Role = role;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }


    }
}
