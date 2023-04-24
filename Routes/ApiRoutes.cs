namespace Stayin.Auth;

/// <summary>
/// Routes for different API endpoints in the application
/// </summary>
public class ApiRoutes
{
    /// <summary>
    /// Retrieve user info route
    /// </summary>
    public const string GetUserById = "/users/byId/{userId}";
    
    /// <summary>
    /// Update user route
    /// </summary>
    public const string UpdateUser = "/users/update/{userId}";

    /// <summary>
    /// Login route
    /// </summary>
    public const string Login = "/login";
    
    /// <summary>
    /// Register route
    /// </summary>
    public const string SignUp = "/signup";
    
    /// <summary>
    /// Route to request authorization from the auth service
    /// </summary>
    public const string RequestAuthorization = "/requestAuthorization";
    
    // TODO: comment this stuff
    public const string DeleteUser = "/users/delete/{userId}";
    public const string GetAllUsers = "/users/all";
    public const string GetUserByUsername = "/users/byUsername/{username}";

}
