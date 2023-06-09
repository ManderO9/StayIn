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
    
    /// <summary>
    /// Route to get all available users
    /// </summary>
    public const string GetAllUsers = "/users/all/{page}";

    /// <summary>
    /// Route to delete a user using his id
    /// </summary>
    public const string DeleteUser = "/users/delete/{userId}";

    /// <summary>
    /// Route to get user info using his username
    /// </summary>
    public const string GetUserByUsername = "/users/byUsername/{username}";

    /// <summary>
    /// Route to get reservation infos for a specific user
    /// </summary>
    public const string GetReservationsForUser = "/reservations/byUserId/{userId}";
    
    /// <summary>
    /// Route to get house publications created by the specified user
    /// </summary>
    public const string GetPublicationsForUser = "/publications/byUserId/{userId}";

}
