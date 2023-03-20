namespace Stayin.Auth;

/// <summary>
/// The model that contains data required to create a user
/// </summary>
public class UserCreationModel
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
    /// The password of the user
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// The phone number of the user
    /// </summary>
    public string? PhoneNumber { get; set; }
}
