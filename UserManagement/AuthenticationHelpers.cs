using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Stayin.Auth;

/// <summary>
/// Helper class for authenticating users
/// </summary>
public class AuthenticationHelpers
{
    /// <summary>
    /// Gets the Jwt token validation parameters that we have set for this application
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    /// <exception cref="UnreachableException"></exception>
    public static TokenValidationParameters GetValidationParameters(IConfiguration configuration)
    {
        // Create a new instance of the validation parameters
        return new TokenValidationParameters()
        {
            // Validate that the issuer is correct
            ValidateIssuer = true,

            // Validate that the audience is correct
            ValidateAudience = true,

            // Validate the token has not expired yet
            ValidateLifetime = true,

            // Validate the signed hash is correct
            ValidateIssuerSigningKey = true,

            // Set the valid value of the issuer and the audience
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],

            // The secret key used to sign the token
            IssuerSigningKey = new SymmetricSecurityKey(
                   Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? throw new UnreachableException())),
        };
    }

    /// <summary>
    /// Authenticates a token, if it is valid it returns the claims principal of the valid user, 
    /// otherwise returns null.
    /// </summary>
    /// <param name="token">The token to authenticate</param>
    /// <param name="configuration">The configuration provider for the application</param>
    /// <returns></returns>
    public static ClaimsPrincipal? AuthenticateJwtToken(string? token, IConfiguration configuration)
    {
        // Create the Jwt validator
        var validator = new JwtSecurityTokenHandler();

        // Get the validation parameters for the token
        var validationParameters = GetValidationParameters(configuration);

        // If the validator can read the token
        if(validator.CanReadToken(token))
        {
            try
            {
                // Get the claims principal using the validator
                var principal = validator.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                // If we got here then the token is valid, return validated principal to the caller
                return principal;
            }
            // If there was an exception, the token is invalid
            catch(Exception e)
            {
                _ = e;
                // Return null meaning there was no valid user
                return null;
            }
        }

        // Return null, the token is not valid
        return null;
    }
}
