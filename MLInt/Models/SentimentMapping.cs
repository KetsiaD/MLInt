using Microsoft.ML.Data;

namespace MLInt.Models;

public class SentimentMapping
{
    [ColumnName("PredictedLabel")]
    public string Prediction { get; set; } // True for positive sentiment, false for negative
    // You might have another property for predicted label, or if it was a regression task, you might have a score
    
   
}
