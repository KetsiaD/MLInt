using System.Diagnostics;
using Python.Runtime;
using Microsoft.AspNetCore.Mvc;
using MLInt.Models;
using Microsoft.AspNetCore.Components.Forms;


namespace MLInt.Controllers
{
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

                // Save the uploaded file
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploadsPath); // Ensure the directory exists
                var filePath = Path.Combine(uploadsPath, uploadedFile.FileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }

                // Path for the generated CSV file
                var csvFileName = $"{Path.GetFileNameWithoutExtension(uploadedFile.FileName)}_vader_results.csv";
                var csvFilePath = Path.Combine(uploadsPath, csvFileName);

                Runscript("VADER",filePath,csvFilePath);
               
                Console.WriteLine(csvFilePath);
                if (await WaitForFileCreationAsync(csvFilePath, 60))
                {
                    return File(System.IO.File.ReadAllBytes(csvFilePath), "text/csv", csvFileName);
                }
                else
                {
                    _logger.LogError($"CSV file not found at: {csvFilePath}");
                    return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
                }
                
            }

            return View("Index", model);
        }
        static void Runscript(string scripname, string inputFilePath, string outputFilePath){
            Runtime.PythonDLL =  "/opt/homebrew/Cellar/python@3.12/3.12.6/Frameworks/Python.framework/Versions/3.12/lib/libpython3.12.dylib";
            PythonEngine.Initialize();
            using (Py.GIL()){
                dynamic sys = Py.Import("sys");
                sys.path.append("/Users/ketsiadusenge/Desktop/Capstone/MLInt/MLInt/Controllers");

                var pythonscript = Py.Import(scripname);
                dynamic pyInputFilePath = new PyString(inputFilePath);
                dynamic pyOutputFilePath = new PyString(outputFilePath);
                pythonscript.InvokeMethod("analyze_and_generate_csv",new PyObject[]{pyInputFilePath,pyOutputFilePath});
            }
        }

        public IActionResult Index()
        {
            return View();
        }
        private async Task<bool> WaitForFileCreationAsync(string filePath, int timeoutInSeconds)
        {
            var startTime = DateTime.UtcNow;
            while (!System.IO.File.Exists(filePath))
            {
                if ((DateTime.UtcNow - startTime).TotalSeconds > timeoutInSeconds)
                {
                    return false;
                }
                await Task.Delay(1000); // Wait for 1 second before checking again
            }
            return true;
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
}
