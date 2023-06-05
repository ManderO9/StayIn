namespace Stayin.Auth;

/// <summary>
/// The model for retrieve user info
/// </summary>
public class UserReadModel
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
    /// The phone number of the user
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// The roles of the user
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// The number of reservations created by this user
    /// </summary>
    public int? ReservationCount { get; set; }

    /// <summary>
    /// The number of publications created by this user
    /// </summary>
    public int? PublicationsCount { get; set; }
}
