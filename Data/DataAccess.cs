using Microsoft.EntityFrameworkCore;

namespace Stayin.Auth;

/// <inheritdoc/>
public class DataAccess : IDataAccess
{
    // TODO: try locking the database access,
    // cuz it could be used somewhere else during the same time and cause an exception


    #region Private Members

    /// <summary>
    /// The Db context of our application
    /// </summary>
    private ApplicationDbContext mDbContext;

    #endregion

    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="databaseContext">The context of the database to use</param>
    public DataAccess(ApplicationDbContext databaseContext)
    {
        mDbContext = databaseContext;
    }

    #endregion

    #region Interface Implementation

    /// <inheritdoc/>
    public async Task<bool> AddToConsumedEventsIfNotAlreadyAsync(string eventId)
    {
        // Check if the message exists in the database
        var exists = await mDbContext.ConsumedEvents.AnyAsync(x => x.EventId == eventId);

        // If it does exist
        if(exists)
            // Return false
            return false;

        // Otherwise, add it to the database
        mDbContext.ConsumedEvents.Add(new BaseEvent() { EventId = eventId, PublishedTime = DateTimeOffset.UtcNow });

        // Save the changes
        await mDbContext.SaveChangesAsync();

        // return true
        return true;
    }

    /// <inheritdoc/>
    public Task<bool> EnsureCreatedAsync() => mDbContext.Database.EnsureCreatedAsync();

    #endregion
}
