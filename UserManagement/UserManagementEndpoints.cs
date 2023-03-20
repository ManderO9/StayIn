using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Stayin.Auth;

/// <summary>
/// Contains endpoints for managing users
/// </summary>
public class UserManagementEndpoints
{

    #region Public Methods

    /// <summary>
    /// Adds different endpoints for handling users to the web application
    /// </summary>
    /// <param name="app">The application to add endpoints to</param>
    public static void AddEndpoints(WebApplication app)
    {
        // Create a new instance of the current class
        var instance = new UserManagementEndpoints();

        // Add login endpoint
        app.MapPost(ApiRoutes.Login, instance.LoginHandler);
    }

    
    public async Task<ApiResponse<string>> LoginHandler(IConfiguration configuration)//,[FromBody] UserLoginModel userInfo)
    {
        // TODO: check validity of the user info and is correct in the database
        var user = new ApplicationUser();
        user.UserName = "hossem";
        await Task.Delay(0);


        // Create claims to put in our token
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim(ClaimTypes.NameIdentifier, user.UserName),
        };

        // Create the credentials used to generate the token
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
            SecurityAlgorithms.HmacSha256);

        // Generate the Jwt token
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            signingCredentials: credentials,
            expires: DateTime.Now.AddDays(5));

        // Return a successful response containing the token
        return new ApiResponse<string>()
        {
            Body = new JwtSecurityTokenHandler().WriteToken(token),
            Successful = true
        };
    }

    #endregion

}
