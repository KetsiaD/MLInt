using Microsoft.ML.Data;

namespace MLInt.Models;

public class SentimentMapping
{
    [ColumnName("PredictedLabel")]
    public string Prediction { get; set; }
    [ColumnName("Score")] // True for positive sentiment, false for negative
    public float[] Score { get; set; }        // Probabilities for each class
    
   
}
