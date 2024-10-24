using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.ML;
using Microsoft.ML.Data;
using MLInt.Models;
using static Microsoft.ML.DataOperationsCatalog;

public class TfidfSentimentPrediction
{
    private ITransformer? _trainedModel;

    

public async Task TrainModel()
{
    try
    {
        var mlContext = new MLContext();
        var dataView = mlContext.Data.LoadFromTextFile<InputModel>(
            "/Users/ketsiadusenge/Desktop/Capstone/MLInt/MLInt/Training Dataset/train.csv", separatorChar: ',', hasHeader: true);
        var smalldata = mlContext.Data.TakeRows(dataView, 3000);
        var trainTestSplit = mlContext.Data.TrainTestSplit(smalldata, testFraction: 0.2);
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
                                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
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

        await Task.Delay(5000); 


    }catch (Exception ex){
        Console.WriteLine(ex.ToString());

    }
}

public async Task<string> predictedSentiment(string input)
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
        var cleanedText = Regex.Replace(input, @"[^\w\s]", "").ToLower();
        cleanedText = Regex.Replace(cleanedText, @"\s+", " ").Trim();

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
        var results = new SentimentOutputModel{
            Sentiment = predictionResult.Prediction,
        };

        Console.WriteLine($"Prediction Result: {results.Sentiment}"); 

        return results.Sentiment;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"In predicting, this is the error: {ex.Message}");
        Console.WriteLine(ex.StackTrace); 
        throw; 
    }
}



}





