namespace Stayin.Core;

/// <summary>
/// Contains information of a house to be rented by other users
/// </summary>
public class HousePublication
{
    /// <summary>
    /// The unique identifier of this entity
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// The id of the user that has created this publication
    /// </summary>
    public required string CreatorId { get; set; }

    /// <summary>
    /// The status of the publication (pending, approved, archived, deleted, hidden)
    /// </summary>
    public required HouseStatus Status { get; set; }

    /// <summary>
    /// The title of the house publication
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The description of the house publication
    /// </summary>
    public string? Description { get; set; }


}
