namespace Stayin.Auth;

/// <summary>
/// Routes for different API endpoints in the application
/// </summary>
public class ApiRoutes
{
    // TODO: comment this stuff
    public const string DeleteUser = "/users/delete/{userId}";
    public const string UpdateUser = "/users/update/{userId}";
    public const string GetAllUsers = "/users/all";
    public const string GetUserById = "/users/byId/{userId}";
    public const string GetUserByUsername = "/users/byUsername/{username}";
    public const string Login = "/login";
    public const string SignUp = "/signup";

    public const string RequestAuthorization = "/requestAuthorization";
}
