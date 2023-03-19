namespace Stayin.Auth;

/// <summary>
/// The model that contains data required to create a user
/// </summary>
public class UserCreationModel
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? PhoneNumber { get; set; }
}
