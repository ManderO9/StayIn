using Microsoft.EntityFrameworkCore;

namespace Stayin.Auth;

/// <summary>
/// Contains configuration for different aspect of the database, like foreign constraints, unique keys, relationships etc...
/// </summary>
public class DbConfiguration
{
    /// <summary>
    /// Configures the different relationships and constrains on the entities stored in the database
    /// </summary>
    /// <param name="builder">The model builder to configure</param>
    public static void Configure(ModelBuilder builder)
    {
        builder.Entity<BaseEvent>().HasKey(x => x.EventId);

        // TODO: configure database models and relationships
    }
}
