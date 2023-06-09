using System.ComponentModel.DataAnnotations;

namespace Stayin.Core;

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
    /// The id of the house publication that we created the reservation from
    /// </summary>
    public required string HousePublicationId { get; set; }

    /// <summary>
    /// The id of the renter
    /// </summary>
    public required string RenterId { get; set; }

    /// <summary>
    /// The date at which the rental starts
    /// </summary>
    public string? StartedDate { get; set; }

    /// <summary>
    /// The date at which the rental ends
    /// </summary>
    public string? EndedDate { get; set; }

    /// <summary>
    /// The title of the publication that this rental has been created from
    /// </summary>
    public string ? PublicationTitle { get; set; }
}
