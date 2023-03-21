namespace Stayin.Auth;

/// <summary>
/// Contains all the roles in our application
/// </summary>
public class ApplicationRoles
{
    /// <summary>
    /// The role of the application administrator
    /// </summary>
    public const string AdminRole = "admin";

    /// <summary>
    /// The role of a user who can rent property
    /// </summary>
    public const string RenterRole = "renter";

    /// <summary>
    /// The role of a user who has property and puts it for rental
    /// </summary>
    public const string LandlordRole = "landlor";
}
