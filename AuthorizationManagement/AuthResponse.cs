namespace Stayin.Auth;

/// <summary>
/// Represents a response to an authorization request
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// The action to do after the authorization request
    /// </summary>
    public required AuthAction Action { get; set; }
}
