// File: Controllers/HomeController.cs
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MLInt.Models;
using MLInt.Analyzers; // Include the service namespace
using System.Text;

namespace MLInt.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly VaderSentimentAnalysis _vadersentimentanalysis;

        public HomeController(ILogger<HomeController> logger, VaderSentimentAnalysis sentimentAnalysisService)
        {
            _logger = logger;
            _vadersentimentanalysis = sentimentAnalysisService; // Initialize the service
        }

        [HttpPost]
        public async Task<IActionResult> Upload(UploadModel model)
        {

            var uploadedFile = Request.Form.Files["uploadedFile"];
            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                model.FileName = uploadedFile.FileName;
                

                // Save the uploaded file
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploadsPath); // Ensure the directory exists
                var filePath = Path.Combine(uploadsPath, uploadedFile.FileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }

                // Use the service to analyze sentiment using the file path
                var sentimentResult = _vadersentimentanalysis.AnalyzeSentiment(filePath);
                TempData["SentimentResult"] = Newtonsoft.Json.JsonConvert.SerializeObject(sentimentResult);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                return View("SentimentResult", sentimentResult); // Display the result in a new view

            }

            return View("Index", model);
        }
        

        // Action method to handle the CSV file download
        public IActionResult DownloadCsv()
        {
            // Retrieve sentiment result from TempData
            if (TempData["SentimentResult"] != null)
            {
                var sentimentResultJson = TempData["SentimentResult"].ToString();
                var sentimentResult = Newtonsoft.Json.JsonConvert.DeserializeObject<SentimentResultViewModel>(sentimentResultJson);

                // Generate CSV content in memory
                var csvContent = new StringBuilder();
                csvContent.AppendLine("PositiveScore,NegativeScore,NeutralScore,CompoundScore");
                csvContent.AppendLine($"{sentimentResult.PositiveScore},{sentimentResult.NegativeScore},{sentimentResult.NeutralScore},{sentimentResult.CompoundScore}");

                // Return CSV as a file to download, with no permanent storage
                var fileBytes = Encoding.UTF8.GetBytes(csvContent.ToString());
                return File(fileBytes, "text/csv", $"SentimentResults_{DateTime.Now:yyyyMMddHHmmss}.csv");
            }

            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult SentimentResult(SentimentResultViewModel model)
        {
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
