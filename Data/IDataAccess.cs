namespace Stayin.Auth;

/// <summary>
/// Handles all operations related to the database
/// </summary>
public interface IDataAccess
{
    /// <summary>
    /// Makes sure that the database has been created
    /// </summary>
    /// <returns><see langword="true"/> if the database has been newly created, <see langword="false"/> if not</returns>
    public Task<bool> EnsureCreatedAsync();

    /// <summary>
    /// If the passed in id already exists in the table of consumed events nothing happens and false is returned,
    /// otherwise the event is added to the consumed events using the passed in id.
    /// </summary>
    /// <param name="eventId">The id of the event to add</param>
    /// <returns><see langword="true"/> if the event has not been already consumed,
    /// or <see langword="false"/> if it was already consumed</returns>
    public Task<bool> AddToConsumedEventsIfNotAlreadyAsync(string eventId);
}