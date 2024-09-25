
using MLInt.Analyzers;

var builder = WebApplication.CreateBuilder(args);
  // Initialize Python runtime

builder.Services.AddScoped<VaderSentimentAnalysis>(); // Add this line

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Serve static files
app.UseStaticFiles();


// Map the default controller route
app.MapDefaultControllerRoute();
// app.Lifetime.ApplicationStopping.Register(() =>
// {
//     PythonEngine.Shutdown();  // Shutdown Python runtime when the app stops
// });

app.Run();
