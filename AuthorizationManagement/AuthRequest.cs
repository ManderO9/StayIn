namespace Stayin.Auth;

/// <summary>
/// An authorization request for a specific resource and an action
/// </summary>
public class AuthRequest
{
    /// <summary>
    /// The Jwt token to validate if needed
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// The path of the resource to validate
    /// </summary>
    public required string Path { get; set; }
}
