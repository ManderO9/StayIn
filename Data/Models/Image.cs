using System.ComponentModel.DataAnnotations;

namespace Stayin.Auth;

/// <summary>
/// An image that is associated to a house publication
/// </summary>
public class Image
{
    /// <summary>
    /// The unique identifier of this image
    /// </summary>
    [Key]
    public required string Id { get; set; }

    /// <summary>
    /// The id of the house publication that this image was posted on
    /// </summary>
    public required string HousePublicationId { get; set; }

}
