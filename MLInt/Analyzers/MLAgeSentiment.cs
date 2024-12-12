using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.ML;
using Microsoft.ML.Data;
using MLInt.Models;
using static Microsoft.ML.DataOperationsCatalog;

public class MlSentimentAnalyzer
{

    

public async Task TrainModel()
{
    try
    {
        //I need to clean my data, figured there are some nulls.
        // I need to return not just the overall sentiment but the postives, negatives, and neutral values.
        var mlContext = new MLContext();
        var dataView = mlContext.Data.LoadFromTextFile<InputModel>(
            "/Users/ketsiadusenge/Desktop/Capstone/MLInt/MLInt/Training Dataset/train.csv", separatorChar: ',', hasHeader: true);
     var allData = mlContext.Data.CreateEnumerable<InputModel>(dataView, reuseRowObject: false)
    .Where(data => !string.IsNullOrEmpty(data.SelectedText) && !string.IsNullOrEmpty(data.Sentiment)) 
    .ToList(); 

var positiveSamples = allData.Where(data => data.Sentiment == "positive").Take(4000);
var negativeSamples = allData.Where(data => data.Sentiment == "negative").Take(4000);
var neutralSamples = allData.Where(data => data.Sentiment == "neutral").Take(2000);


var sampledData = positiveSamples
    .Concat(negativeSamples)
    .Concat(neutralSamples)
    .Select(data =>
    {
        data.SelectedText = CleanText(data.SelectedText); 
        return data;
    }).ToList(); 

Console.WriteLine($"{sampledData}");
var cleanedDataView = mlContext.Data.LoadFromEnumerable(sampledData);
        var trainTestSplit = mlContext.Data.TrainTestSplit(cleanedDataView, testFraction: 0.2);
        var trainSet = trainTestSplit.TrainSet;
        var testSet = trainTestSplit.TestSet;
        Console.WriteLine("Dataset Processed");
        // Data processing pipeline
        var dataProcessPipeline = mlContext.Transforms.Conversion.MapValueToKey("Label")
            .Append(mlContext.Transforms.Text.FeaturizeText(
                outputColumnName: "Features", 
                inputColumnName: nameof(InputModel.SelectedText)));
        Console.WriteLine("Data Process Pipeline done");     
        var trainer = mlContext.MulticlassClassification
                    .Trainers
                    .SdcaMaximumEntropy(
                    labelColumnName: "Label", featureColumnName: "Features");
        Console.WriteLine("Trainer done")    ; 
            
        
        var trainingPipeline = dataProcessPipeline
                                .Append(trainer)
                                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"))
                                .Append(mlContext.Transforms.CopyColumns(outputColumnName:"Score", inputColumnName:"Score"));
          Console.WriteLine("training Pipeline done") ;    
        var trainedModel = trainingPipeline.Fit(trainSet);
        var predictions = trainedModel.Transform(testSet);
        Console.WriteLine("Predictions for training done");     

        var metrics = mlContext.MulticlassClassification.Evaluate(predictions, "Label");
        Console.WriteLine($"Macro accuracy: {metrics.MacroAccuracy:P2}");
        Console.WriteLine($"Micro accuracy: {metrics.MicroAccuracy:P2}");
        Console.WriteLine($"Log loss: {metrics.LogLoss:P2}");
        var modelPath = "/Users/ketsiadusenge/Desktop/Capstone/MLInt/MLInt/Training Dataset/model.zip";
        mlContext.Model.Save(trainedModel, trainSet.Schema, modelPath);

        


    }catch (Exception ex){
        Console.WriteLine(ex.ToString());

    }
}

public async Task<SentimentOutputModel> predictedSentiment(string input)
{
    try
    {
        var mlContext = new MLContext();

        // Check if the model file exists
        var modelPath = "/Users/ketsiadusenge/Desktop/Capstone/MLInt/MLInt/Training Dataset/model.zip";
        if (!System.IO.File.Exists(modelPath))
        {
            throw new FileNotFoundException($"Model file not found at {modelPath}");
        }

        ITransformer loadedModel;
        try
        {
            loadedModel = mlContext.Model.Load(modelPath, out var modelInputSchema);
            Console.WriteLine("Model loaded successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error loading model:");
            Console.WriteLine(ex.Message);
            throw;
        }

        // Creating prediction engine
        var predEngine = mlContext.Model.CreatePredictionEngine<InputModel, SentimentMapping>(loadedModel);

        // Cleaning the input text
        var cleanedText = CleanText(input);

        Console.WriteLine($"Text to predict: '{cleanedText}'");

        // Preparing the input for prediction
        var textanalysis = new InputModel
        {
            SelectedText = cleanedText
        };

        if (string.IsNullOrEmpty(textanalysis.SelectedText))
        {
            throw new ArgumentException("Input text cannot be null or empty.");
        }

        Console.WriteLine($"InputModel SelectedText: '{textanalysis.SelectedText}'");

        // Making a prediction
        SentimentMapping predictionResult;
        try
        {
            predictionResult = predEngine.Predict(textanalysis);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error during prediction:");
            Console.WriteLine(ex.Message);
            throw;
        }
        

        // Check prediction result
        if (predictionResult == null)
        {
            throw new InvalidOperationException("Prediction result is null.");
        }
        var positiveProbability = predictionResult.Score[0];
        var negativeProbability = predictionResult.Score[1];
        var neutralProbability = predictionResult.Score[2];
        var weightedSum = positiveProbability - negativeProbability;

        var sumOfProbabilities = positiveProbability + negativeProbability + neutralProbability;
        var compoundScore = sumOfProbabilities > 1e-6? weightedSum / sumOfProbabilities : 0;


        var results = new SentimentOutputModel
{
    Sentiment = predictionResult.Prediction,
    PositiveProbability = positiveProbability,
    NegativeProbability = negativeProbability,
    NeutralProbability = neutralProbability,
    CompoundScore = compoundScore
};

        

        Console.WriteLine($"Prediction Result: {results.Sentiment}"); 

        return results;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"In predicting, this is the error: {ex.Message}");
        Console.WriteLine(ex.StackTrace); 
        throw; 
    }}
    public static string CleanText(string text)
{
    // Lowercase
    text = text.ToLower();
    
    
    text = Regex.Replace(text, @"[^\w\s]", "");
    
    //stop words
    var stopWords = new HashSet<string> { "the", "is", "at", "which", "on" };
    text = string.Join(" ", text.Split(' ').Where(word => !stopWords.Contains(word)));
    
    //  extra whitespaces
    text = Regex.Replace(text, @"\s+", " ").Trim();
    
    return text;
}

}









