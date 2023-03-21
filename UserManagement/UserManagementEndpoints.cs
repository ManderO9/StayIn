using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
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
        app.MapPost(ApiRoutes.Login, instance.LogInHandler);

        // Add sign up endpoint
        app.MapPost(ApiRoutes.SignUp, instance.SignUpHandler);

    }

    /// <summary>
    /// Handles login for users, makes sure the the passed in info is correct and returns a Jwt token if successful
    /// </summary>
    /// <param name="configuration">The configuration provider for our application</param>
    /// <param name="userInfo">The user info to log in</param>
    /// <param name="userManager">The user manager to retrieve, delete, update and login users</param>
    /// <returns></returns>
    public async Task<ApiResponse<string>> LogInHandler(IConfiguration configuration, [FromBody] UserLoginModel userInfo, UserManager<ApplicationUser> userManager)
    {
        // If we have no email
        if(string.IsNullOrEmpty(userInfo.Email))
            // Return an error
            return new ApiResponse<string>() { Errors = new List<string>() { "The passed in email was empty" } };

        // If we have no password
        if(string.IsNullOrEmpty(userInfo.Password))
            // Return an error
            return new ApiResponse<string>() { Errors = new List<string>() { "The passed in password was empty" } };

        // Get the user from the database
        var user = await userManager.FindByEmailAsync(userInfo.Email);

        // If we have no user
        if(user is null)
            // Return a failed login
            return new ApiResponse<string>() { Errors = new List<string>() { "Invalid email/password combination" } };

        // Check if the passed in password is valid
        var validPassword = await userManager.CheckPasswordAsync(user, userInfo.Password);

        // If the password was not valid
        if(!validPassword)
            // Return a failed login
            return new ApiResponse<string>() { Errors = new List<string>() { "Invalid email/password combination" } };

        // Create the claims to put in our token
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim(ClaimTypes.NameIdentifier, user.UserName ?? throw new UnreachableException()),
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
        };
    }

    /// <summary>
    /// Handles user registration
    /// </summary>
    /// <param name="configuration">The configuration provider for the application</param>
    /// <param name="userInfo">The info of the user we want to create</param>
    /// <param name="userManager">The user manager to retrieve, delete, update, create users, etc...</param>
    /// <returns></returns>
    /// <exception cref="UnreachableException"></exception>
    public async Task<ApiResponse<string>> SignUpHandler(IConfiguration configuration, [FromBody] UserCreationModel userInfo, UserManager<ApplicationUser> userManager)
    {
        // Create the user to add to the database
        var user = new ApplicationUser()
        {
            UserName = userInfo.Username,
            Email = userInfo.Email,
            PhoneNumber = userInfo.PhoneNumber
        };

        // Create the list of errors for password validation
        var passwordErrors = new List<string>();

        // For each password validator
        foreach(var validator in userManager.PasswordValidators)
        {
            // Check if the password is valid 
            var passwordValidation = await validator.ValidateAsync(userManager, user, userInfo.Password);

            // If password validation failed
            if(!passwordValidation.Succeeded)
                // Add the errors to the list of errors
                foreach(var error in passwordValidation.Errors)
                    passwordErrors.Add(error.Description);
        }

        // If there was any errors
        if(passwordErrors.Any())
            // Return a failed sign up with the errors
            return new ApiResponse<string>() { Errors = passwordErrors };

        // Try create the user in the database
        var result = await userManager.CreateAsync(user);

        // If we failed to create the user 
        if(!result.Succeeded)
            // Return an error
            return new ApiResponse<string>() { Errors = result.Errors.Select(x => x.Description).ToList() };

        // If we got here the user has been created in the database and the password is valid

        // Try add the password to the user
        result = await userManager.AddPasswordAsync(user, userInfo.Password!);

        // If we failed
        if(!result.Succeeded)
            // Return an error
            return new ApiResponse<string>() { Errors = result.Errors.Select(x => x.Description).ToList() };

        // Add default roles of "landlord" and "renter" to this user
        await userManager.AddToRolesAsync(user, new[] {ApplicationRoles.LandlordRole, ApplicationRoles.RenterRole});

        // The user is valid and created successfully, now we try generate the token to return

        // Create the claims to put in our token
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim(ClaimTypes.NameIdentifier, user.UserName ?? throw new UnreachableException()),
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
        };
    }

    #endregion


}
