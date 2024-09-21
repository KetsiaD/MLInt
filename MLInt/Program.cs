using Python.Runtime;

var builder = WebApplication.CreateBuilder(args);
  // Initialize Python runtime


// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Serve static files
app.UseStaticFiles();

// Map the default controller route
app.MapDefaultControllerRoute();
app.Lifetime.ApplicationStopping.Register(() =>
{
    PythonEngine.Shutdown();  // Shutdown Python runtime when the app stops
});

app.Run();
