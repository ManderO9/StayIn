namespace Stayin.Core;

/// <summary>
/// The status of a house publication
/// </summary>
public enum HouseStatus
{
    /// <summary>
    /// The publication has been created but not yet approved by the admin
    /// </summary>
    Pending,

    /// <summary>
    /// The publican has been approved by the admin
    /// </summary>
    Approved,

    /// <summary>
    /// The publication has been delete/rejected by the admin
    /// </summary>
    Deleted,
    
    /// <summary>
    /// The publication has been set to be hidden by it's creator
    /// </summary>
    Hidden,

    /// <summary>
    /// The publication has been archived
    /// </summary>
    Archived,
}
