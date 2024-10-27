using Data.Dtos;
using Data.Enums;

namespace Core.UserServices
{
    public interface IUserService
    {
        Task<string> Login(LoginDto loginDto);
        Task<bool> Register(UserDto userDto);
        Task<bool> UpdateUserRole(Guid userId, UserRoleEnum role);
    }
}