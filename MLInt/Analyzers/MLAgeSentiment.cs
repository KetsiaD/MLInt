using Microsoft.ML;
using Microsoft.ML.Data;
using MLInt.Models;
using static Microsoft.ML.DataOperationsCatalog;

public class TfidfSentimentPrediction
{
    private readonly MLContext _mlContext;
    private ITransformer? _trainedModel;

    public bool IsTrained => _trainedModel != null;

    public TfidfSentimentPrediction()
    {
        _mlContext = new MLContext();
    }


public ITransformer TrainModel(string trainingDataPath)
{
    try
    {
        // Step 1: Load the data
        var data = _mlContext.Data.LoadFromTextFile<InputModel>(trainingDataPath, hasHeader: true, separatorChar: ',');

        // Step 2: Convert IDataView to IEnumerable
        var allData = _mlContext.Data.CreateEnumerable<InputModel>(data, reuseRowObject: false).ToList();

        // Step 3: Filter out neutral and null sentiments
        var filteredData = allData
            .Where(input => !string.IsNullOrEmpty(input.Sentiment) && 
                            (input.Sentiment.Equals("positive", StringComparison.OrdinalIgnoreCase) || 
                             input.Sentiment.Equals("negative", StringComparison.OrdinalIgnoreCase)))
            .ToList();

        // Step 4: Convert back to IDataView
        var filteredDataView = _mlContext.Data.LoadFromEnumerable(filteredData);

        // Step 5: Define the data processing pipeline with custom mapping
        var dataProcessPipeline = _mlContext.Transforms.CustomMapping<InputModel, LabelModel>(
            (input, output) =>
            {
                output.Features = input.SelectedText; // This will be featurized later
                output.Label = input.Sentiment.Equals("positive", StringComparison.OrdinalIgnoreCase);
            }, contractName: null)
            .Append(_mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Features", inputColumnName: nameof(InputModel.SelectedText)));

        // Step 6: Set the training algorithm
        var trainer = _mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
            labelColumnName: "Label", // This refers to the new label created in the mapping
            featureColumnName: "Features");

        // Create the full training pipeline
        var trainingPipeline = dataProcessPipeline.Append(trainer);

        // Step 7: Split the data into training and test sets
        TrainTestData trainTestSplit = _mlContext.Data.TrainTestSplit(filteredDataView, testFraction: 0.2);
        IDataView trainingData = trainTestSplit.TrainSet;
        IDataView testData = trainTestSplit.TestSet;

        // Step 8: Train the model
        _trainedModel = trainingPipeline.Fit(trainingData);

        // Optional: Evaluate the model
        var predictions = _trainedModel.Transform(testData);
        var metrics = _mlContext.BinaryClassification.Evaluate(data: predictions, labelColumnName: "Label", scoreColumnName: "Score");

        // Output metrics
        Console.WriteLine($"Accuracy: {metrics.Accuracy:P2}");
        Console.WriteLine($"AUC: {metrics.AreaUnderRocCurve:P2}");
        Console.WriteLine($"F1 Score: {metrics.F1Score:P2}");

        return _trainedModel;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during model training: {ex.Message}");
        return null; // Return null if an error occurs
    }
}



    public bool Predict(string inputText)
{
    // Check if the model is trained
    if (_trainedModel == null)
    {
        throw new InvalidOperationException("The model must be trained before making predictions.");
    }

    // Prepare the input for prediction
    var input = new InputModel { SelectedText = inputText };

    // Create the prediction engine
    var predictor = _mlContext.Model.CreatePredictionEngine<InputModel, SentimentMapping>(_trainedModel);

    // Make the prediction
    var prediction = predictor.Predict(input);

    // Create the SentimentOutputModel based on the prediction
    var result = new SentimentOutputModel
    {
        Sentiment = prediction.SentimentBool,  // Assuming this is a boolean (true for Positive)
        // If you have additional info, you can add more fields here
    };

    return result.Sentiment;  // Return the SentimentOutputModel
}


}
