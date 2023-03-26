using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http.Json;
using Stayin.Auth;
using System.Text;
using System.Text.Json;
using System.Diagnostics;



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

    var message = new AuthRequest() { Path = "/hello, new message", Token = Guid.NewGuid().ToString("N") };
    eventBus.Publish(message);



    return message;
});

app.MapGet("/getMessages", async () =>
{

    var a = new RabbitMQEventBus();


    var output = "";

    await a.StartConsuming(
        (sender, eventArgs) =>
        {
            var body = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

            output += body + Environment.NewLine;
            switch(eventArgs.BasicProperties.Type)
            {
                case nameof(AuthRequest):
                    var newEvent = JsonSerializer.Deserialize<AuthRequest>(body);
                    break;
                case nameof(AuthResponse):
                    var newEve = JsonSerializer.Deserialize<AuthResponse>(body);
                    break;
                default:
                    Debugger.Break();
                    throw new NotImplementedException();
            }
            // TODO: negative acknowledge the message
            
        });

    await Task.Delay(500);

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
