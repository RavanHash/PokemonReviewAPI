using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PokemonReviewApp.Dto;
using PokemonReviewApp.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PokemonReviewApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly UserManager<IdentityUser> userManager;
    private readonly IConfiguration configuration;

    public AuthenticationController(UserManager<IdentityUser> userManager, IConfiguration configuration)
    {
        this.userManager = userManager;
        this.configuration = configuration;
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto requestDto)
    {
        if (ModelState.IsValid)
        {
            var userExist = await userManager.FindByEmailAsync(requestDto.Email);

            if (userExist != null)
            {
                return BadRequest(new AuthenticationResult()
                {
                    Result = false,
                    Errors = new List<string>() { "Email alredy exist" }
                });
            }

            var newUser = new IdentityUser()
            {
                Email = requestDto.Email,
                UserName = requestDto.Name
            };

            var isCreated = await userManager.CreateAsync(newUser, requestDto.Password);
            if (isCreated.Succeeded)
            {
                var token = GenerateJwtToken(newUser);

                return Ok(new AuthenticationResult()
                {
                    Result = true,
                    Token = token
                });
            }

            return BadRequest(new AuthenticationResult()
            {
                Result = false,
                Errors = new List<string>() { "Server error" } // how to display identity password error message?
            });

        }

        return BadRequest(ModelState);
    }

    private string GenerateJwtToken(IdentityUser user)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.UTF8.GetBytes(configuration.GetSection("JwtConfig:Secret").Value);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("Id", user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Email, value:user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString())
            }),

            Expires = DateTime.Now.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        var token = jwtTokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = jwtTokenHandler.WriteToken(token);

        return jwtToken;
    }
}
