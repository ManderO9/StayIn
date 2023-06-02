namespace Stayin.Auth;

/// <summary>
/// An authorization action obtained from and authorization request,
/// that will define what we should do next after getting the result of the request
/// </summary>
public enum AuthAction
{
    /// <summary>
    /// Authorization was granted, proceed with the requested action
    /// </summary>
    Proceed = 0,

    /// <summary>
    /// Login is required, redirect to the login page
    /// </summary>
    Login,

    /// <summary>
    /// You do not have the privileges to access the requested resource
    /// </summary>
    AccessDenied,

    /// <summary>
    /// The action/resource requested does not exit
    /// </summary>
    NotFound
}
