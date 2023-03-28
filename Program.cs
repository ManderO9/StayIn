using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Identity;
using Stayin.Auth;
using System.Diagnostics;
using System.Text.Json;



// Create application builder
var builder = WebApplication.CreateBuilder(args);


// TODO: delete later
var corsPolicy = "someCorsPolicy";
builder.Services.AddCors((options) =>
{
    options.AddPolicy(corsPolicy,
        policy =>
        {
            policy.WithOrigins("*").WithHeaders("*").WithMethods("*");
        }
        );
});






// Remove the default camel case naming convention for properties
builder.Services.Configure<JsonOptions>(config =>
{
    config.SerializerOptions.PropertyNamingPolicy = null;
});

// Add application services
builder.Services.AddServices(builder.Configuration);

// Build the application
var app = builder.Build();


// Add endpoints
UserManagementEndpoints.AddEndpoints(app);
AuthorizationEndpoints.AddEndpoints(app);

app.Map("/", () => "hello world!");







app.Map("/create", (IEventBus eventBus) =>
{
    var message = new UserCreatedEvent()
    {
        EventId = Guid.NewGuid().ToString("N"),
        PublishedTime = DateTimeOffset.UtcNow,
        UserId = Guid.NewGuid().ToString("N"),
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

app.Map("/get", async (HttpContext context) =>
{
    var messages = await context.RequestServices.GetRequiredService<IEventBus>().GetAllMessages();

    var newMessages = new List<string>();

    messages.ForEach(message =>
    {
        switch(message.messageType)
        {
            case nameof(UserCreatedEvent):
                newMessages.Add(JsonSerializer.Serialize(JsonSerializer.Deserialize<UserCreatedEvent>(message.message.Span)));
                break;

            case nameof(UserDeletedEvent):
                newMessages.Add(JsonSerializer.Serialize(JsonSerializer.Deserialize<UserDeletedEvent>(message.message.Span)));
                break;

            default:
                Debugger.Break();
                throw new NotImplementedException();

        }
    });

    return newMessages;
});






// Make sure that the database exists
using var scope = app.Services.CreateScope();
var newlyCreated = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreatedAsync();

// If we have newly created eventBus database 
if(newlyCreated)
{
    // Add all roles to it
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    await roleManager.CreateAsync(new ApplicationRole() { Name = ApplicationRoles.AdminRole });
    await roleManager.CreateAsync(new ApplicationRole() { Name = ApplicationRoles.LandlordRole });
    await roleManager.CreateAsync(new ApplicationRole() { Name = ApplicationRoles.RenterRole });
}



// TODO: delete later
app.UseCors(corsPolicy);


// Run the app
app.Run();
