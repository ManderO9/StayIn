using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http.Json;
using Stayin.Auth;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;



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



app.MapGet("/createMessage", () =>
{

    var eventBus = new RabbitMQEventBus();

    var message = new UserCreatedEvent()
    {
        EventId = Guid.NewGuid().ToString("N"),
        PublishedTime = DateTimeOffset.UtcNow,
        UserId = Guid.NewGuid().ToString("N"),
    };
    eventBus.Publish(message);

    return message;
});

app.MapGet("/getMessages", async (ApplicationDbContext mDbContext) =>
{
    var a = new RabbitMQEventBus();
    var output = "";
    var lockObj = new object();

    var closed = false;

    await a.StartConsuming(
        async (eventArgs) =>
        {
            if(closed)
            
                return;

            // Convert the message to base event
            var baseEvent = JsonSerializer.Deserialize<BaseEvent>(eventArgs.Body.Span)!;

            lock(lockObj)
            {
                // Check that the message has not been handled before
                var handled = mDbContext.ConsumedEvents.Any(x => x.EventId == baseEvent.EventId);

                // If it wasn't
                if(!handled)
                {
                    // Add it to the list of handled messages
                    mDbContext.ConsumedEvents.Add(baseEvent);
                    mDbContext.SaveChanges();

                    // Switch the type of the event
                    switch(eventArgs.BasicProperties.Type)
                    {
                        case nameof(UserCreatedEvent):
                            var newEvent = JsonSerializer.Deserialize<UserCreatedEvent>(eventArgs.Body.Span);
                            output += "Event Id: " + newEvent?.EventId + Environment.NewLine;
                            break;

                        default:
                            Debugger.Break();
                            throw new NotImplementedException();

                    }
                }
            }
        });

    await Task.Delay(5000);

    closed = true;
    await a.StopConsuming();

    return output;
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
