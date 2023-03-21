using Microsoft.AspNetCore.Identity;
using Stayin.Auth;



// Create application builder
var builder = WebApplication.CreateBuilder(args);

// Add application services
builder.Services.AddServices(builder.Configuration);

// Build the application
var app = builder.Build();


// Add endpoints
UserManagementEndpoints.AddEndpoints(app);
AuthorizationEndpoints.AddEndpoints(app);
app.MapGet("/", () => "Hello World!");


// Make sure that the database exists
using var scope = app.Services.CreateScope();
var newlyCreated = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreatedAsync();

// If we have newly created a database 
if(newlyCreated)
// Add all roles to it
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    await roleManager.CreateAsync(new ApplicationRole() { Name = ApplicationRoles.AdminRole });
    await roleManager.CreateAsync(new ApplicationRole() { Name = ApplicationRoles.LandlordRole });
    await roleManager.CreateAsync(new ApplicationRole() { Name = ApplicationRoles.RenterRole });
}

// Run the app
app.Run();
