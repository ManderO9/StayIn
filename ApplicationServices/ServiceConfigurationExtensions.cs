using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Stayin.Auth;

/// <summary>
/// Extension methods for configuring the required services by our application
/// </summary>
public static class ServiceConfigurationExtensions
{
    /// <summary>
    /// Adds all the services needed by our application to the <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="services">The DI container to add the services to</param>
    /// <returns></returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add database context
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            // Use SQL server as backend database
            options.UseSqlServer(configuration.GetConnectionString("Default"));
        });

        // Add data access service
        services.AddScoped<IDataAccess, DataAccess>();

        // Add identity service to the application
        services.AddIdentity<ApplicationUser, ApplicationRole>()

            // Set the persistent data store used by the identity system
            .AddEntityFrameworkStores<ApplicationDbContext>()

            // Add token providers for resetting passwords, changing emails, etc...
            .AddDefaultTokenProviders();

        // Configure identity options
        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 5;
            options.Password.RequiredUniqueChars = 2;

            options.User.RequireUniqueEmail = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
        });


        // Add authentication using Jwt bearer tokens
        services.AddAuthentication().AddJwtBearer(options =>
        {
            // Set token validation parameters
            options.TokenValidationParameters = AuthenticationHelpers.GetValidationParameters(configuration);
        });

        // Add queue consumer service
        services.AddHostedService<QueueConsumerService>();

        // Add RabbitMQ event bus
        services.AddTransient<IEventBus, RabbitMQEventBus>(provider => new RabbitMQEventBus(
                provider.GetRequiredService<IConfiguration>()["RabbitMQ:Queue"]!,
                provider.GetRequiredService<IConfiguration>()["RabbitMQ:Uri"]!));

        // Return the services for chaining
        return services;
    }
}
