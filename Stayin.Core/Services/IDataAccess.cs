namespace Stayin.Core;

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
    /// <remarks>
    ///     Saves the changes automatically to the database
    /// </remarks>
    public Task<bool> AddToConsumedEventsIfNotAlreadyAsync(string eventId);

    /// <summary>
    /// Returns a list of all available event ids in the database
    /// </summary>
    /// <returns></returns>
    public Task<List<string>> LoadExistingEventIdsAsync();

    /// <summary>
    /// Creates a new Event in the datastore
    /// </summary>
    /// <param name="newEvent">The event to create</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    /// <remarks>
    ///     Changes are not saved in the database until <see cref="SaveChangesAsync"/> is called
    /// </remarks>
    public Task CreateEventAsync(BaseEvent newEvent);

    /// <summary>
    /// Persists the changes in memory to the database
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public Task SaveChangesAsync();

    /// <summary>
    /// Returns a list of available users in the passed in page and the defined size
    /// </summary>
    /// <param name="page">The page to get users from</param>
    /// <param name="size">The size of the page</param>
    /// <returns></returns>
    public Task<List<(ApplicationUser User, List<ApplicationRole> Roles, int ReservationsCount, int PublicationsCount)>> GetUsersInPage(int page, int size);

    /// <summary>
    /// Returns user details using his username
    /// </summary>
    /// <param name="username">The username of the user to return</param>
    /// <returns></returns>
    public Task<(ApplicationUser? User, List<ApplicationRole>? Roles, int ReservationsCount, int PublicationsCount)> GetUserDetails(string username);

    public Task AddUserPaymentDetails(PaymentDetails details);

    public Task UpdateUserEmail(string userId, string email);

    public Task DeleteUserDetails(string userId);

    public Task AddRentalAsync(Rental rental);

    public Task CreateHousePublicationAsync(HousePublication housePublication);

    public Task<HousePublication?> GetHousePublicationById(string id);
    
}