// File: Services/SentimentAnalysisService.cs
using VaderSharp2;
using MLInt.Models;
using System.IO;
using System.Text;

namespace MLInt.Analyzers
{
    public class VaderSentimentAnalysis
    {
 
        public SentimentResultViewModel AnalyzeSentiment(string filePath)
        {
          
            string textContent;
            using (var reader = new StreamReader(filePath, Encoding.UTF8))
            {
                textContent = reader.ReadToEnd();
            }

           
            var analyzer = new SentimentIntensityAnalyzer();
            var sentimentScores = analyzer.PolarityScores(textContent);

           
            var sentimentResult = new SentimentResultViewModel
            {
                PositiveScore = sentimentScores.Positive,
                NegativeScore = sentimentScores.Negative,
                NeutralScore = sentimentScores.Neutral,
                CompoundScore = sentimentScores.Compound
            };

            return sentimentResult;
        }
    }
}
