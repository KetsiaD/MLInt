var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Serve static files
app.UseStaticFiles();

// Map the default controller route
app.MapDefaultControllerRoute();

app.Run();
