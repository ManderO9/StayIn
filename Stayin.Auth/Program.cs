using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Identity;
using Stayin.Auth;
using Stayin.Core;


// Create application builder
var builder = WebApplication.CreateBuilder(args);

//// TODO: delete later
//var corsPolicy = "someCorsPolicy";
//builder.Services.AddCors((options) =>
//    { options.AddPolicy(corsPolicy, policy => { policy.WithOrigins("*").WithHeaders("*").WithMethods("*"); }); });



// Remove the default camel case naming convention for properties
builder.Services.Configure<JsonOptions>(config =>
{
    config.SerializerOptions.PropertyNamingPolicy = null;
});

// Add application services
builder.Services.AddApplicationServices(builder.Configuration);

// Build the application
var app = builder.Build();

// Add endpoints
UserManagementEndpoints.AddEndpoints(app);
AuthorizationEndpoints.AddEndpoints(app);

// Make sure that the database exists
using var scope = app.Services.CreateScope();
var newlyCreated = await scope.ServiceProvider.GetRequiredService<IDataAccess>().EnsureCreatedAsync();

// If we have newly created eventBus database 
if(newlyCreated)
{
    // Add all roles to it
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    _ = await roleManager.CreateAsync(new ApplicationRole() { Name = ApplicationRoles.AdminRole });
    _ = await roleManager.CreateAsync(new ApplicationRole() { Name = ApplicationRoles.LandlordRole });
    _ = await roleManager.CreateAsync(new ApplicationRole() { Name = ApplicationRoles.RenterRole });
}


// TODO: delete later
//app.UseCors(corsPolicy);
app.Map("/", () => "hello world!");



// TODO: delete test endpoints
app.Map("/create", (IEventBus eventBus) =>
{
    var message = new UserCreatedEvent()
    {
        EventId = Guid.NewGuid().ToString("N"),
        PublishedTime = DateTimeOffset.UtcNow,
        UserId = Guid.NewGuid().ToString("N"),
        Username= "hossem",
        Email = "hossem@gmail.com",
        PhoneNumber = "432oz4232975"
    };
    eventBus.Publish(message);

    var otherMessage = new UserDeletedEvent()
    {
        EventId = Guid.NewGuid().ToString("N"),
        PublishedTime = DateTimeOffset.UtcNow,
        UserId = Guid.NewGuid().ToString("N"),
    };
    eventBus.Publish(otherMessage);

    return (message, otherMessage);
});

app.Map("/get", async (IEventBus eventBus, IDataAccess db) =>
{
    return await eventBus.GetNewEvents(db);
});

app.Map("/getall", async (IEventBus eventBus) =>
{
    return await eventBus.GetAllEvents();
});






// Run the app
app.Run();
