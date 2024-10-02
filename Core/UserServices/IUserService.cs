using Data.Dtos;

namespace Core.UserServices
{
    public interface IUserService
    {
        Task<string> Login(LoginDto loginDto);
        Task<bool> Register(UserDto userDto);
    }
}