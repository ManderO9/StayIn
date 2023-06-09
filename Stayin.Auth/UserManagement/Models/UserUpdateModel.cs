namespace Stayin.Auth;

/// <summary>
/// The model for updating a user
/// </summary>
public class UserUpdateModel
{
    /// <summary>
    /// The username of the user
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// The email of the user
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// The phone number of the user
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// The description a user gives about himself
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The id of the profile image for this user
    /// </summary>
    public string? ProfileImageId { get; set; }
}
