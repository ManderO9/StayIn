using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Stayin.Auth;

/// <summary>
/// The data context for the application database
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    #region Public Properties

    /// <summary>
    /// Contains information of a house to be rented by other users
    /// </summary>
    public DbSet<HousePublication> HousePublications { get; set; }

    /// <summary>
    /// Represents a relationship between a house and an option, used to know which options
    /// are available in which houses
    /// </summary>
    public DbSet<HousePublicationOptions> HousePublicationOptions { get; set; }

    /// <summary>
    /// Images that are associated to house publications
    /// </summary>
    public DbSet<Image> Images { get; set; }

    /// <summary>
    /// Represents a relationship between a user and a house publication, 
    /// if it exists that means that the user has liked the house publication
    /// </summary>
    public DbSet<Like> Likes { get; set; }

    /// <summary>
    /// House rentals that the users have created
    /// </summary>
    public DbSet<Rental> Rentals { get; set; }

    /// <summary>
    /// The messages between the landlord and the renter
    /// </summary>
    public DbSet<Message> Messages { get; set; }

    #endregion

    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="options">The options to configure the database context</param>
    public ApplicationDbContext(DbContextOptions options) :base(options){}

    #endregion

    #region Override methods

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Configure different constraints in our application
        DbConfiguration.Configure(builder);

        // Call base function
        base.OnModelCreating(builder);
    }
    
    #endregion
}
