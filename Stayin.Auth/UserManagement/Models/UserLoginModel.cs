namespace Stayin.Auth;

/// <summary>
/// The model that contains data required to log a user in
/// </summary>
public class UserLoginModel
{
    /// <summary>
    /// The email of the user
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// The password of the user
    /// </summary>
    public string? Password { get; set; }
}
