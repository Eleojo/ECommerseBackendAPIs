using Core.UserServices;
using Data.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerseBackendApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController: ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register-user")]
        public async Task<IActionResult> RegisterUser(UserDto userDto)
        {
            var result = await _userService.Register(userDto);
            if (result)
            {
                return Ok("User created successfully");
            }

            return BadRequest("User creation failed");  
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (loginDto == null)
            {
                return BadRequest("Login details are not provided");
            }

            // Call the login service
            var token = await _userService.Login(loginDto);

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Invalid login credentials");
            }

            // Return the generated token if login was successful
            return Ok(new { Token = token });
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("admin-data")]
        public IActionResult GetAdminData()
        {
            // Only accessible by users with Admin role
            return Ok("This is admin data.");
        }


    }
}
