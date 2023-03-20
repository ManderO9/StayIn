using System.Security.Claims;

namespace Stayin.Auth;

/// <summary>
/// Handles authorization to different resources in the application
/// </summary>
public class AuthorizationEndpoints
{

    /// <summary>
    /// Adds different endpoints for handling users to the web application
    /// </summary>
    /// <param name="app">The application to add endpoints to</param>
    public static void AddEndpoints(WebApplication app)
    {
        // Create a new instance of the current class
        var instance = new AuthorizationEndpoints();


        // TODO: delete later, this is just for testing
        app.MapGet("/auth", instance.HandleRequest);
    }

    public string HandleRequest(string? token, IConfiguration configuration)
    {
        // Try authenticate the token and get the user claims
        var userClaims = AuthenticationHelpers.AuthenticateJwtToken(token, configuration);

        // If the user claims are not null, the token is valid
        if(userClaims is not null)
        {
            return "user is valid, his name is: " + userClaims.FindFirst(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        }

        return "invalid token";
    }
}
