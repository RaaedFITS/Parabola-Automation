using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Start the Python script when the application starts
StartPythonScript();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Method to start the Python script
void StartPythonScript()
{
    try
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = "fetch_flows.py", // Ensure this is the correct path to your Python script
            UseShellExecute = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = true
        };

        var process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();
        Console.WriteLine("Python script started successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error starting Python script: {ex.Message}");
    }
}
