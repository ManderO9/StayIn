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
await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreatedAsync();


// Run the app
app.Run();
