using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MLInt.Models;
using MLInt.Analyzers;
using System.Text;

namespace MLInt.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly VaderSentimentAnalysis _vadersentimentanalysis;
        private readonly MlSentimentAnalyzer _mlsentimentanalyzer;

        public HomeController(ILogger<HomeController> logger, 
            VaderSentimentAnalysis sentimentAnalysisService, 
            MlSentimentAnalyzer mlsentimentanalyzer)
        {
            _logger = logger;
            _vadersentimentanalysis = sentimentAnalysisService;
            _mlsentimentanalyzer = mlsentimentanalyzer;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(UploadModel model)
        {
            CombinedResultViewModel combinedResult = new CombinedResultViewModel();
            var uploadedFile = Request.Form.Files["uploadedFile"];
            var selectedAlgorithm = Request.Form["algorithm"];

            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                model.FileName = uploadedFile.FileName;

                // Saving the uploaded file
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploadsPath); // Ensure directory exists
                var filePath = Path.Combine(uploadsPath, uploadedFile.FileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }

                
                
               
            try
            {
                
                if (selectedAlgorithm == "Vader")
                {
                    Console.WriteLine("imA HERE IN VADER");
                    // Call Vader sentiment analysis
                    combinedResult.SentimentResult = _vadersentimentanalysis.AnalyzeSentiment(filePath);
                }
                else if (selectedAlgorithm == "Algorithm1")
                {
                    Console.WriteLine("I am in MLsentiment analyzer");
                    string textToPredict = await System.IO.File.ReadAllTextAsync(filePath);
                    Console.WriteLine($"Text to predict: {textToPredict}");
                    
                    var modelPath = "/Users/ketsiadusenge/Desktop/Capstone/MLInt/MLInt/Training Dataset/model.zip";
                    
                    if (!System.IO.File.Exists(modelPath)) 
                    {
                        await _mlsentimentanalyzer.TrainModel();
                        Console.WriteLine("traineddata");
                    }
                    try
                        {
                            Console.WriteLine("we are here");

                            if (combinedResult == null)
                            {
                                Console.WriteLine("combinedResult is null.");
                                return null; // Prevent further execution
                            }
                            combinedResult = new CombinedResultViewModel{
                                UserOutput = await _mlsentimentanalyzer.predictedSentiment(textToPredict)
                            };

                            Console.WriteLine("Prediction happened");
                            Console.WriteLine(combinedResult.UserOutput.Sentiment);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"An error occurred: {ex.Message}");
                           
                        }
                    
                    Console.WriteLine(combinedResult.UserOutput.Sentiment);
                }
                else if (selectedAlgorithm == "Algorithm2"){
                    try{
                        Console.WriteLine("Entering Text Summarizing algorithm");
                        var textforsumarry = await System.IO.File.ReadAllTextAsync(filePath);
                        Console.WriteLine(textforsumarry);
                        string summaryText = TextRankSummarizer.Summarize(textforsumarry);

                        combinedResult = new CombinedResultViewModel{
                            TextSummary = new TextSummaryModel { Summary = summaryText }
                        };

                    }catch(Exception ex){
                        Console.WriteLine($"Error occured in summary:{ex.Message}");
                    }
                   
                }
                else
                {
                    ModelState.AddModelError("", "Invalid algorithm selected.");
                    return View("Index", model);
                }

                TempData["CombinedResult"] = Newtonsoft.Json.JsonConvert.SerializeObject(combinedResult);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View("Index", model);
            }
            finally
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

                return View("SentimentResult", combinedResult);
            }

            ModelState.AddModelError("", "No file uploaded.");
            return View("Index", model);
        }

        public IActionResult DownloadCsv()
        {
            if (TempData["CombinedResult"] != null)
            {
                var combinedResultJson = TempData["CombinedResult"].ToString();
                var combinedResult = Newtonsoft.Json.JsonConvert.DeserializeObject<CombinedResultViewModel>(combinedResultJson);

                var csvContent = new StringBuilder();
                if (combinedResult.SentimentResult != null)
                {
                    csvContent.AppendLine("PositiveScore,NegativeScore,NeutralScore,CompoundScore");
                    csvContent.AppendLine($"{combinedResult.SentimentResult.PositiveScore},{combinedResult.SentimentResult.NegativeScore},{combinedResult.SentimentResult.NeutralScore},{combinedResult.SentimentResult.CompoundScore}");
                }
                else if (combinedResult.UserOutput != null)
                {
                    csvContent.AppendLine("PositiveScore,NegativeScore,NeutralScore,CompoundScore");
                    csvContent.AppendLine($"{combinedResult.UserOutput.PositiveProbability},{combinedResult.UserOutput.NegativeProbability},{combinedResult.UserOutput.NeutralProbability},{combinedResult.UserOutput.CompoundScore}");
                }

                // Return CSV as a file for download without permanent storage
                var fileBytes = Encoding.UTF8.GetBytes(csvContent.ToString());
                return File(fileBytes, "text/csv", $"Results_{DateTime.Now:yyyyMMddHHmmss}.csv");
            }

            return RedirectToAction("Index");
        }
        public IActionResult DownloadSummary()
{
    if (TempData["CombinedResult"] != null)
    {
        var combinedResultJson = TempData["CombinedResult"].ToString();
        var combinedResult = Newtonsoft.Json.JsonConvert.DeserializeObject<CombinedResultViewModel>(combinedResultJson);

        var summaryContent = new StringBuilder();
        if (combinedResult.TextSummary != null)
        {
            summaryContent.AppendLine($"Summary: {combinedResult.TextSummary.Summary}");
        }
        else
        {
            summaryContent.AppendLine("No summary available.");
        }

        var fileBytes = Encoding.UTF8.GetBytes(summaryContent.ToString());
        return File(fileBytes, "text/plain", $"Summary_{DateTime.Now:yyyyMMddHHmmss}.txt");
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

        public IActionResult SentimentResult(CombinedResultViewModel model)
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
