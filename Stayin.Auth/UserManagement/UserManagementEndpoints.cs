using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Server;
using Stayin.Core;
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

        // Add user update endpoint
        app.MapPost(ApiRoutes.UpdateUser, instance.UpdateUserHandler);

        // Add retrieve user by id endpoint
        app.MapGet(ApiRoutes.GetUserById, instance.GetUserById);

        // Add get all users endpoint
        app.MapGet(ApiRoutes.GetAllUsers, instance.GetAllUsers);

        // Add delete user endpoint
        app.MapGet(ApiRoutes.DeleteUser, instance.DeleteUserById);
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
            new Claim(JwtRegisteredClaimNames.NameId, user.Id),
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
        await userManager.AddToRolesAsync(user, new[] { ApplicationRoles.LandlordRole, ApplicationRoles.RenterRole });

        // The user is valid and created successfully, now we try generate the token to return

        // Create the claims to put in our token
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim(JwtRegisteredClaimNames.NameId, user.Id),
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
    /// Handles updating user info
    /// </summary>
    /// <param name="userInfo">The info of the user to update</param>
    /// <param name="userId">The id of the user to update</param>
    /// <param name="userManager">The user manager to handle user updating</param>
    /// <returns>An <see cref="ApiResponse"/> with the new user info in it's body if successful, or errors if failed</returns>
    public async Task<ApiResponse<UserUpdateModel>> UpdateUserHandler([FromBody] UserUpdateModel userInfo, string userId, UserManager<ApplicationUser> userManager)
    {
        // Try get the user using it's id
        var user = await userManager.FindByIdAsync(userId);

        // If it doesn't exist
        if(user is null)
            // Return an error
            return new() { Errors = new() { "The user that you want to update does not exit" } };

        // If the username is not empty
        if(!string.IsNullOrEmpty(userInfo.Username))
            // Update it
            user.UserName = userInfo.Username;

        // If the phone number is not empty
        if(!string.IsNullOrEmpty(userInfo.PhoneNumber))
            // Update it
            user.PhoneNumber = userInfo.PhoneNumber;

        // If the email is not empty
        if(!string.IsNullOrEmpty(userInfo.Email))
            // Update it
            user.Email = userInfo.Email;

        // If the description is not empty
        if(!string.IsNullOrEmpty(userInfo.Description))
            // Update it
            user.Description = userInfo.Description;

        // If the image id is not empty
        if(!string.IsNullOrEmpty(userInfo.ProfileImageId))
            // Update it
            user.ProfileImageId = userInfo.ProfileImageId;

        // Try save the changes 
        var result = await userManager.UpdateAsync(user);

        // If the update was successful
        if(result.Succeeded)
            // Return a success response containing the user
            return new() { Body = userInfo };

        // Otherwise, return an error
        return new() { Errors = result.Errors.Select(x => x.Description).ToList() };
    }

    /// <summary>
    /// Returns info of a user that has the specified id
    /// </summary>
    /// <param name="userId">The id of the user to get details for</param>
    /// <param name="userManager">The user manager to retrieve the user from the database</param>
    /// <returns>An <see cref="ApiResponse"/> with the user info in it's body if successful, or errors if failed</returns>
    public async Task<ApiResponse<UserReadModel>> GetUserById(string userId, UserManager<ApplicationUser> userManager)
    {
        // Try get the user using it's id
        var user = await userManager.FindByIdAsync(userId);

        // If it doesn't exist
        if(user is null)
            // Return an error
            return new() { Errors = new() { $"The user with id: {userId} does not exit" } };

        // Return the retrieved user
        return new() { Body = new() { Username = user.UserName, Email = user.Email, PhoneNumber = user.PhoneNumber, Id = user.Id, ProfileImageId = user.ProfileImageId } };
    }

    /// <summary>
    /// Returns list of users available in a specific page
    /// </summary>
    /// <param name="page">The page of users to get</param>
    /// <param name="dataAccess">Data access service</param>
    /// <returns>List of available users, or an empty list if none available for the passed in page</returns>
    public async Task<List<UserReadModel>> GetAllUsers([FromRoute] int page, IDataAccess dataAccess)
    {
        // Get users from database
        var users = await dataAccess.GetUsersInPage(page, 15);

        // Map users to user read model and return them
        return users.Select(x => new UserReadModel()
        {
            Email = x.User.Email,
            PhoneNumber = x.User.PhoneNumber,
            Username = x.User.UserName,
            Type = x.Roles.Select(x => x.Name).Aggregate((a, b) => a + "/" + b),
            PublicationsCount = x.PublicationsCount,
            ReservationCount = x.ReservationsCount,
            Id = x.User.Id
        }).ToList();
    }

    /// <summary>
    /// Deletes a user from the backing store using it's id
    /// </summary>
    /// <param name="userId">The id of the user to delete</param>
    /// <param name="userManager">User manager to manager users</param>
    /// <returns>An <see cref="ApiResponse"/>Containing a list of errors if failed, or none if successful</returns>
    public async Task<ApiResponse> DeleteUserById([FromRoute] string userId, UserManager<ApplicationUser> userManager)
    {
        // If user id is empty
        if(string.IsNullOrEmpty(userId))

            // Return an error
            return new ApiResponse() { Errors = new List<string> { "User id can't be empty" } };

        // Get the user to delete
        var user = await userManager.FindByIdAsync(userId);

        // If we have no user
        if(user is null)
            // Return an error
            return new ApiResponse() { Errors = new List<string> { $"User with id:'{userId}' does not exist" } };


        // Delete the user
        var result = await userManager.DeleteAsync(user);

        // If successful
        if(result.Succeeded)

            // Return success response
            return new ApiResponse();

        // Otherwise, return an error
        return new ApiResponse() { Errors = result.Errors.Select(x => x.Description).ToList() };
    }

    /// <summary>
    /// Returns user details using his username, or null if the user does not exist
    /// </summary>
    /// <param name="username">The username of the user to return the details for</param>
    /// <param name="dataAccess">Data access service</param>
    /// <returns></returns>
    public async Task<UserReadModel?> GetUserByUsername([FromRoute] string username, IDataAccess dataAccess)
    {
        // Get user details from database
        var userDetails = await dataAccess.GetUserDetails(username);

        // If we got no user
        if(userDetails.User is null)
            // Return null
            return null;

        // Map user details to user read model 
        var output = new UserReadModel()
        {
            Email = userDetails.User.Email,
            PhoneNumber = userDetails.User.PhoneNumber,
            Username = userDetails.User.UserName,
            Type = userDetails.Roles?.Select(x => x.Name).Aggregate((a, b) => a + "/" + b),
            PublicationsCount = userDetails.PublicationsCount,
            ReservationCount = userDetails.ReservationsCount,
            Id = userDetails.User.Id
        };

        // Return the user details
        return output;
    }


    #endregion
}
