using Microsoft.EntityFrameworkCore;
using Stayin.Core;

namespace Stayin.Auth;

/// <inheritdoc/>
public class DataAccess : IDataAccess
{
    // TODO: try locking the database access,
    // cuz it could be used somewhere else during the same time and cause an exception
    // or maybe not cuz each scope will have his own instance, so no need to lock it
    // will see


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

    public Task AddUserPaymentDetails(PaymentDetails details) => throw new NotImplementedException();
    public Task DeleteUserDetails(string userId) => throw new NotImplementedException();
    public Task UpdateUserEmail(string userId, string email) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task CreateEventAsync(BaseEvent newEvent)
     => Task.FromResult(mDbContext.ConsumedEvents.Add(newEvent));

    /// <inheritdoc/>
    public Task<bool> EnsureCreatedAsync() => mDbContext.Database.EnsureCreatedAsync();

    /// <inheritdoc/>
    public Task<List<string>> LoadExistingEventIdsAsync()
        => mDbContext.ConsumedEvents.Select(x => x.EventId).ToListAsync();

    /// <inheritdoc/>
    public Task SaveChangesAsync() => mDbContext.SaveChangesAsync();

    /// <inheritdoc/>
    public async Task<List<(ApplicationUser User, List<ApplicationRole> Roles, int ReservationsCount, int PublicationsCount)>> GetUsersInPage(int page, int size)
    {
        // Get the list of users requested
        var users = await mDbContext.Users.Skip(page * size).Take(size).ToListAsync();

        // Create the list to return
        var output = new List<(ApplicationUser User, List<ApplicationRole> Roles, int ReservationsCount, int PublicationsCount)>();

        // For each user
        foreach(var user in users)
        {
            // Get the number of reservations created
            var reservationCount = await mDbContext.Rentals.CountAsync(x => x.RenterId == user.Id);

            // Get the number of house publications created
            var housePublicationsCount = await mDbContext.HousePublications.CountAsync(x => x.CreatorId == user.Id);

            // Create query to get user roles
            var rolesQuery = from role in mDbContext.Roles join 
                             userHasRole in mDbContext.UserRoles on role.Id equals userHasRole.RoleId
                             where userHasRole.UserId == user.Id select role;

            // Get user roles
            var roles = await rolesQuery.ToListAsync();

            // Add this entry to the output
            output.Add((user, roles, reservationCount, housePublicationsCount));
        }

        // Return the list of users with details
        return output;
    }


    #endregion
}
