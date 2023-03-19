using System.ComponentModel.DataAnnotations;

namespace Stayin.Auth;

/// <summary>
/// Represents a house rental
/// </summary>
public class Rental
{
    /// <summary>
    /// The unique identifier of this rental
    /// </summary>
    [Key]
    public required string Id { get; set; }

    /// <summary>
    /// The id of the landlord 
    /// </summary>
    public required string LandlordId { get; set; }

    /// <summary>
    /// The id of the renter
    /// </summary>
    public required string RenterId { get; set; }
}
