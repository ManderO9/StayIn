using Microsoft.EntityFrameworkCore;

namespace Stayin.Core;

/// <summary>
/// Represents a relationship between a user and a house publication, 
/// if it exists that means that the user has liked the house publication
/// </summary>
[PrimaryKey(nameof(UserId), nameof(PublicationId))]
public class Like
{
    /// <summary>
    /// The id of the user that has liked the publication
    /// </summary>
    public required string UserId { get; set; }

    /// <summary>
    /// The id of the publication that the user has liked
    /// </summary>
    public required string PublicationId { get; set; }
}
