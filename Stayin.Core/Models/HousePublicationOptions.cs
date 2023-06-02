using Microsoft.EntityFrameworkCore;

namespace Stayin.Core;

/// <summary>
/// Represents a relationship between a house and an option, used to know which options
/// are available in which houses
/// </summary>
[PrimaryKey(nameof(HousePublicationId), nameof(OptionId))]
public class HousePublicationOptions
{
    /// <summary>
    /// The id of the house that has this option
    /// </summary>
    public required string HousePublicationId { get; set;}

    /// <summary>
    /// The id of the option that is associated to the current house
    /// </summary>
    public required string OptionId { get; set; }
}
