using Microsoft.AspNetCore.Identity;

namespace Stayin.Core;

/// <summary>
/// The user of our application
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// The description a user gives about himself
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The id of the image that the user displays in his profile
    /// </summary>
    public string? ProfileImageId { get; set; }
}
