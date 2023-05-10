using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace Stayin.Auth;


/// <summary>
/// Handles authorization to different resources in the application
/// </summary>
public class AuthorizationEndpoints
{
    #region Private Members

    /// <summary>
    /// Contains all the handlers for authorization requests
    /// </summary>
    private List<Func<AuthRequest, IServiceProvider, Task<(bool handled, AuthAction action)>>> mAuthHandlers;

    #endregion

    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public AuthorizationEndpoints()
    {
        // Set the handlers for different authorization requests in our application
        mAuthHandlers = new()
        {
            // Login route
            (authRequest, provider) => { 
                // Try match the request path with the login route
                var result = Match(authRequest.Path, ApiRoutes.Login);
        
                // If the route does not match the login path
                if(!result.matched)
                    // Return false, indicating that we did not handle this request
                    return Task.FromResult((false, default(AuthAction)));

                // Otherwise, grant permission to access the resource 
                return Task.FromResult((true, AuthAction.Proceed));
            },

            // Sign up route
            (authRequest, provider) => {
                // Try match the request path with the sign up route
                var result = Match(authRequest.Path, ApiRoutes.SignUp);
        
                // If the route does not match
                if(!result.matched)
                    // Return false, indicating that we did not handle this request
                    return Task.FromResult((false, default(AuthAction)));

                // Otherwise, grant permission to access the resource 
                return Task.FromResult((true, AuthAction.Proceed));
            },

            // Get all users route
            async (authRequest, provider) => {
                // Try match the request path with the get all users route
                var result = Match(authRequest.Path, ApiRoutes.GetAllUsers);

                // If the route does not match
                if(!result.matched)
                    // Return false, indicating that we did not handle this request
                    return (false, default(AuthAction));

                // Try authenticate the token and get the user claims
                var userClaims = AuthenticationHelpers.AuthenticateJwtToken(authRequest.Token, provider.GetRequiredService<IConfiguration>());

                // If the user claims are null, the token is invalid
                if(userClaims is null)
                    // Return login action to the caller
                    return (true, AuthAction.Login);

                // Get the user manager
                var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
                
                // Check if the user has an admin role
                var isAdmin = await userManager.IsInRoleAsync((await userManager.FindByNameAsync(Username(userClaims)))!, ApplicationRoles.AdminRole);

                // If he is an admin, he can get all users, otherwise, he will get rejected 
                return (true, isAdmin ? AuthAction.Proceed : AuthAction.AccessDenied);
            },
           
            // Update user
            async (authRequest, provider) => {
                // Try match the request path with the update user route
                var pathMatchResult = Match(authRequest.Path, ApiRoutes.UpdateUser);

                // If the route does not match
                if(!pathMatchResult.matched)
                    // Return false, indicating that we did not handle this request
                    return (false, default(AuthAction));

                // Try authenticate the token and get the user claims
                var userClaims = AuthenticationHelpers.AuthenticateJwtToken(authRequest.Token, provider.GetRequiredService<IConfiguration>());

                // If the user claims are null, the token is invalid
                if(userClaims is null)
                    // Return login action to the caller
                    return (true, AuthAction.Login);

                // Get the user manager
                var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
                
                // Get the user requesting the update
                var user = await userManager.FindByNameAsync(Username(userClaims));

                // Check if the user has an admin role
                var isAdmin = await userManager.IsInRoleAsync(user!, ApplicationRoles.AdminRole);

                // If he is an admin, he can update the user
                if (isAdmin) return (true, AuthAction.Proceed );
                
                // Otherwise, if the user has the same id as the user he is requesting to update
                if(user!.Id == pathMatchResult.pathParams!.First(x=>x.paramName == "userId").value)
                    // Return success
                    return (true, AuthAction.Proceed);

                // Otherwise, return unauthorized response
                return (true, AuthAction.AccessDenied);
            },
           
            // Get user details by id
            (authRequest, provider) => {
                // Try match the request path with the get user by id route
                var pathMatchResult = Match(authRequest.Path, ApiRoutes.GetUserById);

                // If the route does not match
                if(!pathMatchResult.matched)
                    // Return false, indicating that we did not handle this request
                    return Task.FromResult((false, default(AuthAction)));

                // Try authenticate the token and get the user claims
                var userClaims = AuthenticationHelpers.AuthenticateJwtToken(authRequest.Token, provider.GetRequiredService<IConfiguration>());

                // If the user claims are null, the token is invalid
                if(userClaims is null)
                    // Return login action to the caller
                    return Task.FromResult((true, AuthAction.Login));

                // Otherwise the user is logged in and can access other users info
                return Task.FromResult((true, AuthAction.Proceed));
            }
        };
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Adds different endpoints for handling authorization in our application
    /// </summary>
    /// <param name="app">The application to add endpoints to</param>
    public static void AddEndpoints(WebApplication app)
    {
        // Create a new instance of the current class
        var instance = new AuthorizationEndpoints();

        // TODO: delete later, this is just for testing
        app.MapGet("/auth", (string? token, IConfiguration configuration) =>
        {
            // Try authenticate the token and get the user claims
            var userClaims = AuthenticationHelpers.AuthenticateJwtToken(token, configuration);

            // If the user claims are not null, the token is valid
            if(userClaims is not null)
            {
                return "user is valid, his name is: " + instance.Username(userClaims);
            }

            return "invalid token";
        });

        // Handle authorization requests
        app.MapPost(ApiRoutes.RequestAuthorization, instance.HandleAuthRequest);
    }

    /// <summary>
    /// Handler for authorization requests
    /// </summary>
    /// <param name="authRequest">The authorization request</param>
    /// <returns></returns>
    public async Task<ApiResponse<AuthResponse>> HandleAuthRequest([FromBody] AuthRequest authRequest, IServiceProvider provider)
    {
        // For each authentication handler 
        foreach(var handler in mAuthHandlers)
        {
            // Get the result of running the handle method
            var result = await handler(authRequest, provider);

            // If the request has been handled...
            if(result.handled)
                // Return the action result that we got from the handler
                return new() { Body = new() { Action = result.action } };
        }

        // If no previous handler matched the request route
        // Return not found action, meaning there was no resource/handler for the requested resource
        return new() { Body = new() { Action = AuthAction.NotFound } };
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Returns true if the passed in route matches the pattern
    /// </summary>
    /// <param name="route">The route to compare</param>
    /// <param name="pattern">The pattern to match against</param>
    /// <returns><see cref="true"/> if the route matches the pattern with the list of path parameters if any,
    /// or <see cref="false"/> with a null parameters list if it doesn't match</returns>
    private (bool matched, List<(string paramName, string value)>? pathParams) Match(string route, string pattern)
    {
        // Get all the parts of the pattern separated by /
        var patternParts = pattern.Split("/");

        // Get all the parts of the route separated by /
        var routeParts = route.Split("/");

        // If the number of parts in the route doesn't equal the pattern
        if(routeParts.Length != patternParts.Length)
            // The routes don't match, return false
            return (false, null);

        // Create a list that will contain the path parameters from the route
        List<(string paramName, string value)> pathParams = new();

        // For each part
        for(int i = 0; i < patternParts.Length; i++)
        {
            // If the current pattern part is empty...
            if(string.IsNullOrEmpty(patternParts[i]))
                // Just ignore it
                continue;

            // If this part starts with { and ends with }
            if(patternParts[i].StartsWith("{") && patternParts[i].EndsWith("}"))
            {
                // Check if it is optional (ends with "?}")
                if(patternParts[i].EndsWith("?}"))
                {
                    // Add the parameter value to the parameters list
                    pathParams.Add((patternParts[i].TrimStart('{').TrimEnd('}').TrimEnd('?'), routeParts[i]));

                    // Mark it as valid and go to the next iteration
                    continue;
                }
                // Otherwise, this part is not optional
                else

                // Check that it has a value
                if(!string.IsNullOrEmpty(routeParts[i]))
                    // If it does add it to the path parameters
                    pathParams.Add((patternParts[i].TrimStart('{').TrimEnd('}'), routeParts[i]));
                // Otherwise, if it doesn't
                else
                    // Mark it as invalid and return false
                    return (false, null);

            }
            // Otherwise, if this is not a path parameter 
            else
                // Check that the two values are equal
                if(patternParts[i] != routeParts[i])
                // Return false indicating that they don't match
                return (false, null);
        }

        // The routes match, return true
        return (true, pathParams);
    }

    /// <summary>
    /// Returns the username from the users claims
    /// </summary>
    /// <param name="claims">The claims to get the username from</param>
    /// <returns></returns>
    private string Username(ClaimsPrincipal claims)
        => claims.FindFirst(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? throw new UnreachableException();


    #endregion

}
