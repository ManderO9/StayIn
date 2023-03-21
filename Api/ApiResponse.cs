namespace Stayin.Auth;

/// <summary>
/// The response of an API request
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// Whether the request was successful or failed
    /// </summary>
    public bool Successful => Errors == null || Errors.Count == 0;

    /// <summary>
    /// The errors to display to the user if the request failed
    /// </summary>
    public List<string>? Errors { get; set; }
}

/// <summary>
/// An API response containing data returned in the result
/// </summary>
/// <typeparam name="TData">The type of data to return</typeparam>
public class ApiResponse<TData> : ApiResponse
{
    /// <summary>
    /// The body of the response containing the request data if successful
    /// </summary>
    public TData? Body { get; set; }
}
