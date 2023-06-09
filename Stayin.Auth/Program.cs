using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stayin.Auth;
using Stayin.Core;


// TODO: publish events everytime you create/delete/update a user


// Create application builder
var builder = WebApplication.CreateBuilder(args);

// Remove the default camel case naming convention for properties
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(config =>
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


app.Map("/", () => "Hello from Auth microservice");

app.MapGet(ApiRoutes.GetReservationsForUser, ([FromRoute] string userId, ApplicationDbContext db)
    => db.Rentals.Where(x => x.RenterId == userId));

app.MapGet(ApiRoutes.GetPublicationsForUser, ([FromRoute] string userId, ApplicationDbContext db)
    => db.HousePublications.Where(x => x.CreatorId == userId));

// Run the app
app.Run();
