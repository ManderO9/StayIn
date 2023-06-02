using Microsoft.EntityFrameworkCore;

namespace Stayin.Auth;

/// <summary>
/// The message between the landlord and the renter
/// </summary>
[PrimaryKey(nameof(ReceiverId), nameof(SenderId))]
public class Message
{
    /// <summary>
    /// The id of the message receiver
    /// </summary>
    public required string ReceiverId { get; set; }
    
    /// <summary>
    /// The id of the message sender
    /// </summary>
    public required string SenderId { get; set; }
}
