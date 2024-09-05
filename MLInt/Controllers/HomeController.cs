using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MLInt.Models;
using Newtonsoft.Json;


namespace MLInt.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }
    [HttpPost]
    public async Task<IActionResult> Upload(UploadModel model)
    {
        var uploadedFile = Request.Form.Files["uploadedFile"];
        if (uploadedFile != null && uploadedFile.Length > 0)
        {
            model.FileName = uploadedFile.FileName;

            // Create a JSON string with the file name
            var jsonResult = JsonConvert.SerializeObject(new { FileSubmittedName = model.FileName });
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsPath); // Ensure the directory exists

            var jsonFileName = $"{Path.GetFileNameWithoutExtension(uploadedFile.FileName)}.json";
            var jsonFilePath = Path.Combine(uploadsPath, jsonFileName);

        // Save the JSON file
            await System.IO.File.WriteAllTextAsync(jsonFilePath, jsonResult);

        // Download the JSON file
            return File(System.IO.File.ReadAllBytes(jsonFilePath), "application/json", jsonFileName);

            // Define the file path to save the JSON file
            //var jsonFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", $"{Path.GetFileNameWithoutExtension(uploadedFile.FileName)}.json");

            // Save the JSON file
            //await System.IO.File.WriteAllTextAsync(jsonFilePath, jsonResult);

            // Download the JSON file
            //return File(System.IO.File.ReadAllBytes(jsonFilePath), "application/json", $"{Path.GetFileNameWithoutExtension(uploadedFile.FileName)}.json");
        }

        return View("Index", model);
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}






