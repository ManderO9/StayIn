namespace Stayin.Auth;

/// <summary>
/// Routes for different API endpoints in the application
/// </summary>
public class ApiRoutes
{
    // TODO: comment this stuff
    public const string CreateUser = "/users/create";
    public const string DeleteUser = "/users/delete/{id}";
    public const string UpdateUser = "/users/update/{id}";
    public const string GetAllUsers = "/users/all";
    public const string GetUserById = "/users/byId/{id}";
    public const string GetUserByUsername = "/users/byUsername/{username}";
    public const string Login = "/login";

}
